// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ProtoBuf.Grpc.Server;

namespace Server;

/// <summary>
/// Extension methods for registering gRPC services.
/// </summary>
public static class GrpcServiceRegistration
{
    /// <summary>
    /// Adds auto-discovered gRPC services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAutoDiscoveredGrpcServices(this IServiceCollection services)
    {
        // Load assemblies
        var backendAssembly = Assembly.Load("Backend");

        // Create factory
        var factory = new ServiceImplementationFactory(backendAssembly);

        // Scan for service interfaces
        foreach (var serviceType in ServiceScanner.ScanForServices(Assembly.Load("Interfaces")))
        {
            // Create implementation instance
            var implementation = factory.CreateImplementation(serviceType);

            // Register as singleton
            services.AddSingleton(serviceType, implementation);

            Console.WriteLine($"Registered gRPC service: {serviceType.Name}");
        }

        return services;
    }

    /// <summary>
    /// Maps auto-discovered gRPC services.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IEndpointRouteBuilder MapAutoDiscoveredGrpcServices(this IEndpointRouteBuilder app)
    {
        // Scan for service interfaces
        foreach (var serviceType in ServiceScanner.ScanForServices(Assembly.Load("Interfaces")))
        {
            // Map the service using reflection
            var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(serviceType);

            mapGrpcServiceMethod.Invoke(null, [app]);

            Console.WriteLine($"Mapped gRPC service: {serviceType.Name}");
        }

        return app;
    }
}
