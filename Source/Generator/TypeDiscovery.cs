// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Generator;

/// <summary>
/// Analyzes an assembly to discover types with command/query attributes.
/// </summary>
sealed class TypeDiscovery(Assembly assembly)
{
    const string CommandAttributeTypeName = "CommandAttribute";
    const string ReadModelAttributeTypeName = "ReadModelAttribute";

    readonly Assembly _assembly = assembly;

    public List<DiscoveredType> DiscoverTypes()
    {
        var discoveredTypes = new List<DiscoveredType>();

        foreach (var type in _assembly.GetTypes())
        {
            if (GetTypeKind(type) is not { } kind)
            {
                continue;
            }

            var serviceName = GetServiceName(type);

            discoveredTypes.Add(new DiscoveredType
            {
                Type = type,
                ServiceName = serviceName,
                Kind = kind,
                Namespace = type.Namespace ?? string.Empty
            });
        }

        return discoveredTypes;
    }

    DiscoveredTypeKind? GetTypeKind(Type type)
    {
        foreach (var attribute in type.GetCustomAttributesData())
        {
            var attributeName = attribute.AttributeType.Name;

            if (attributeName == CommandAttributeTypeName)
            {
                return DiscoveredTypeKind.Command;
            }

            if (attributeName == ReadModelAttributeTypeName)
            {
                // Check if return type is ISubject<T> to determine if it's observable
                var handleMethod = type.GetMethod("Handle");
                if (handleMethod != null)
                {
                    var returnType = handleMethod.ReturnType;
                    if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name.StartsWith("ISubject"))
                    {
                        return DiscoveredTypeKind.ObservableQuery;
                    }
                }
                return DiscoveredTypeKind.Query;
            }
        }

        return null;
    }

    string GetServiceName(Type type)
    {
        // Extract service name from namespace: Backend.Products -> Products
        var ns = type.Namespace ?? string.Empty;
        var parts = ns.Split('.');
        return parts.Length > 1 ? parts[^1] : "Default";
    }
}
