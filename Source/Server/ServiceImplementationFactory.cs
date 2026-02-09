// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Server;

/// <summary>
/// Factory for creating service implementations.
/// </summary>
public class ServiceImplementationFactory
{
    readonly Assembly _backendAssembly;
    readonly Dictionary<string, Type> _backendTypeCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceImplementationFactory"/> class.
    /// </summary>
    /// <param name="backendAssembly">The backend assembly containing command and query handlers.</param>
    public ServiceImplementationFactory(Assembly backendAssembly)
    {
        _backendAssembly = backendAssembly;
    }

    /// <summary>
    /// Creates a service implementation for the specified service interface.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <returns>An implementation of the service interface.</returns>
    public TService CreateImplementation<TService>() where TService : class
    {
        return DispatchProxy.Create<TService, ServiceProxy>() as TService ?? throw new InvalidOperationException($"Failed to create proxy for {typeof(TService).Name}");
    }

    /// <summary>
    /// Creates a service implementation for the specified service interface.
    /// </summary>
    /// <param name="serviceType">The service interface type.</param>
    /// <returns>An implementation of the service interface.</returns>
    public object CreateImplementation(Type serviceType)
    {
        var method = typeof(ServiceImplementationFactory).GetMethod(nameof(CreateImplementation), 1, [])!;
        var genericMethod = method.MakeGenericMethod(serviceType);
        return genericMethod.Invoke(this, null)!;
    }

    Type? GetBackendType(string dtoTypeName)
    {
        if (_backendTypeCache.TryGetValue(dtoTypeName, out var cachedType))
        {
            return cachedType;
        }

        var backendTypeName = ServiceScanner.GetBackendTypeName(dtoTypeName);
        var backendType = ServiceScanner.FindBackendType(backendTypeName, _backendAssembly);

        if (backendType != null)
        {
            _backendTypeCache[dtoTypeName] = backendType;
        }

        return backendType;
    }

    /// <summary>
    /// Proxy class that intercepts service method calls.
    /// </summary>
    class ServiceProxy : DispatchProxy
    {
        ServiceImplementationFactory? _factory;

        /// <summary>
        /// Invokes the proxied method.
        /// </summary>
        /// <param name="targetMethod">The method being invoked.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The result of the method invocation.</returns>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }

            _factory ??= new ServiceImplementationFactory(Assembly.Load("Backend"));

            var parameter = args?[0];
            if (parameter == null)
            {
                throw new ArgumentException("Service methods must have at least one parameter");
            }

            var parameterType = parameter.GetType();
            var backendType = _factory.GetBackendType(parameterType.FullName!);

            if (backendType == null)
            {
                throw new InvalidOperationException($"Backend type not found for {parameterType.FullName}");
            }

            // Map DTO to backend type
            var backendInstance = TypeMapper.MapToBackend(parameter, backendType);

            // Find and invoke Handle method
            var handleMethod = backendType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);
            if (handleMethod == null)
            {
                throw new InvalidOperationException($"Handle method not found on {backendType.FullName}");
            }

            var result = handleMethod.Invoke(backendInstance, null);

            // Handle different return types
            var returnType = targetMethod.ReturnType;

            // Handle Task<CommandResult> for commands
            if (returnType == typeof(Task<>).MakeGenericType(typeof(Interfaces.CommandResult)))
            {
                return Task.FromResult(Interfaces.CommandResult.Success);
            }

            // Handle Task<T> for queries
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var taskResultType = returnType.GetGenericArguments()[0];
                
                if (result != null)
                {
                    var mappedResult = TypeMapper.MapToDto(result, taskResultType);
                    return Task.FromResult(mappedResult);
                }

                return Task.FromResult<object?>(null);
            }

            // Handle IAsyncEnumerable<T> for observable queries
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            {
                var elementType = returnType.GetGenericArguments()[0];

                // The result from Handle() is ISubject<T>
                if (result != null)
                {
                    var convertMethod = typeof(ServiceProxy).GetMethod(nameof(ConvertSubjectToAsyncEnumerable), BindingFlags.NonPublic | BindingFlags.Static)!;
                    var genericMethod = convertMethod.MakeGenericMethod(elementType);
                    return genericMethod.Invoke(null, [result])!;
                }
            }

            return result;
        }

        static async IAsyncEnumerable<T> ConvertSubjectToAsyncEnumerable<T>(ISubject<T> subject, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queue = new System.Collections.Concurrent.ConcurrentQueue<T>();
            var semaphore = new SemaphoreSlim(0);

            using var subscription = subject.Subscribe(item =>
            {
                queue.Enqueue(item);
                semaphore.Release();
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                await semaphore.WaitAsync(cancellationToken);

                if (queue.TryDequeue(out var item))
                {
                    // Map backend type to DTO type
                    var mappedItem = (T)TypeMapper.MapToDto(item!, typeof(T));
                    yield return mappedItem;
                }
            }
        }
    }
}
