// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Generator;

/// <summary>
/// Analyzes an assembly to discover types with command/query attributes.
/// </summary>
class TypeDiscovery
{
    const string CommandAttributeTypeName = "CommandAttribute";
    const string QueryAttributeTypeName = "QueryAttribute";
    const string ObservableQueryAttributeTypeName = "ObservableQueryAttribute";
    const string BelongsToAttributeTypeName = "BelongsToAttribute";

    readonly Assembly _assembly;

    public TypeDiscovery(Assembly assembly)
    {
        _assembly = assembly;
    }

    public List<DiscoveredType> DiscoverTypes()
    {
        var discoveredTypes = new List<DiscoveredType>();
        var types = _assembly.GetTypes();

        foreach (var type in types)
        {
            var kind = GetTypeKind(type);
            if (kind == null)
            {
                continue;
            }

            var serviceName = GetServiceName(type);
            if (serviceName == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Type '{type.FullName}' has a command/query attribute but no [BelongsTo] attribute. Skipping.");
                Console.ResetColor();
                continue;
            }

            discoveredTypes.Add(new DiscoveredType
            {
                Type = type,
                ServiceName = serviceName,
                Kind = kind.Value,
                Namespace = type.Namespace ?? string.Empty
            });
        }

        return discoveredTypes;
    }

    DiscoveredTypeKind? GetTypeKind(Type type)
    {
        var attributes = type.GetCustomAttributesData();

        foreach (var attribute in attributes)
        {
            var attributeName = attribute.AttributeType.Name;

            if (attributeName == CommandAttributeTypeName)
            {
                return DiscoveredTypeKind.Command;
            }

            if (attributeName == QueryAttributeTypeName)
            {
                return DiscoveredTypeKind.Query;
            }

            if (attributeName == ObservableQueryAttributeTypeName)
            {
                return DiscoveredTypeKind.ObservableQuery;
            }
        }

        return null;
    }

    string? GetServiceName(Type type)
    {
        var attributes = type.GetCustomAttributesData();

        foreach (var attribute in attributes)
        {
            if (attribute.AttributeType.Name == BelongsToAttributeTypeName)
            {
                if (attribute.ConstructorArguments.Count > 0)
                {
                    return attribute.ConstructorArguments[0].Value?.ToString();
                }
            }
        }

        return null;
    }
}
