// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Groups discovered types by service and validates namespace consistency.
/// </summary>
sealed class ServiceGrouper
{
    public Dictionary<string, List<DiscoveredType>> GroupByService(List<DiscoveredType> types)
    {
        return types.GroupBy(t => t.ServiceName)
                   .ToDictionary(g => g.Key, g => g.ToList());
    }

    public void ValidateNamespaceConsistency(Dictionary<string, List<DiscoveredType>> serviceGroups)
    {
        var errors = new List<string>();

        foreach (var (serviceName, types) in serviceGroups)
        {
            var namespaces = types.Select(t => t.Namespace).Distinct().ToList();

            if (namespaces.Count > 1)
            {
                errors.Add($"\nService '{serviceName}' has types in multiple namespaces:");
                foreach (var ns in namespaces)
                {
                    var typesInNamespace = types.Where(t => t.Namespace == ns).ToList();
                    errors.Add($"  Namespace: {ns}");
                    foreach (var type in typesInNamespace)
                    {
                        errors.Add($"    - {type.Type.Name} ({type.Kind})");
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Error: Namespace mismatch detected!");
            foreach (var error in errors)
            {
                Console.Error.WriteLine(error);
            }
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
