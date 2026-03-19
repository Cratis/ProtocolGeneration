// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Server;

/// <summary>
/// Factory for creating service implementations.
/// </summary>
public class ServiceImplementationFactory(Assembly backendAssembly)
{
    readonly Assembly _backendAssembly = backendAssembly;
    readonly Dictionary<string, Type> _backendTypeCache = new();

    /// <summary>
    /// Creates a service implementation for the specified service interface.
    /// </summary>
    /// <typeparam name="TService">The service interface type.</typeparam>
    /// <returns>An implementation of the service interface.</returns>
    public TService CreateImplementation<TService>()
        where TService : class
    {
        return DispatchProxy.Create<TService, ServiceProxy>();
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
    sealed class ServiceProxy : DispatchProxy
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
            ArgumentNullException.ThrowIfNull(targetMethod);
            ArgumentNullException.ThrowIfNull(args);

            _factory ??= new ServiceImplementationFactory(Assembly.Load("Backend"));

            var parameter = (args.Length == 0 || args[0] is null)
                ? throw new InvalidOperationException("Service methods must have at least one non-null parameter")
                : args[0];

            var parameterType = parameter.GetType();
            var backendType = _factory.GetBackendType(parameterType.FullName!);

            if (backendType is null)
            {
                throw new InvalidOperationException($"Backend type not found for {parameterType.FullName}");
            }

            // Map DTO to backend type
            var backendInstance = TypeMapper.MapToBackend(parameter, backendType);

            // Find and invoke Handle method
            var handleMethod = backendType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException($"Handle method not found on {backendType.FullName}");

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

                if (result is not null)
                {
                    return Task.FromResult(TypeMapper.MapToDto(result, taskResultType));
                }

                return Task.FromResult<object?>(null);
            }

            // Handle IAsyncEnumerable<T> for observable queries
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            {
                var elementType = returnType.GetGenericArguments()[0];

                // The result from Handle() is ISubject<T>
                if (result is not null)
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
            using var semaphore = new SemaphoreSlim(0);

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
                    yield return (T)TypeMapper.MapToDto(item!, typeof(T));
                }
            }
        }
    }
}
