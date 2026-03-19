// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ServiceModel;

namespace Server;

/// <summary>
/// Scans assemblies for service interfaces.
/// </summary>
public static class ServiceScanner
{
    /// <summary>
    /// Scans the specified assembly for service interfaces marked with [ServiceContract].
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>A collection of service interface types.</returns>
    public static IEnumerable<Type> ScanForServices(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsInterface && Attribute.IsDefined(t, typeof(ServiceContractAttribute)));
    }

    /// <summary>
    /// Gets the backend type name from a DTO type name.
    /// </summary>
    /// <param name="dtoTypeName">The DTO type name.</param>
    /// <returns>The backend type name.</returns>
    public static string GetBackendTypeName(string dtoTypeName)
    {
        // DTO: "Interfaces.Products.CreateProduct"
        // Backend: "Backend.Products.CreateProduct"
        if (dtoTypeName.StartsWith("Interfaces.", StringComparison.Ordinal))
        {
            return $"Backend.{dtoTypeName.Substring("Interfaces.".Length)}";
        }
        return dtoTypeName;
    }

    /// <summary>
    /// Finds a backend type by name.
    /// </summary>
    /// <param name="typeName">The type name to find.</param>
    /// <param name="backendAssembly">The backend assembly to search in.</param>
    /// <returns>The backend type, or null if not found.</returns>
    public static Type? FindBackendType(string typeName, Assembly backendAssembly)
    {
        return backendAssembly.GetTypes()
            .FirstOrDefault(t => t.FullName == typeName);
    }
}
