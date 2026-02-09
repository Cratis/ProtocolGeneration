// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Generator;

/// <summary>
/// Main interface generator orchestrator.
/// </summary>
class InterfaceGenerator
{
    readonly string _assemblyPath;
    readonly string _outputDirectory;
    readonly string _baseNamespace;
    readonly int _skipSegments;

    public InterfaceGenerator(string assemblyPath, string outputDirectory, string baseNamespace, int skipSegments)
    {
        _assemblyPath = assemblyPath;
        _outputDirectory = outputDirectory;
        _baseNamespace = baseNamespace;
        _skipSegments = skipSegments;
    }

    public async Task GenerateAsync()
    {
        Console.WriteLine("Protocol Interface Generator");
        Console.WriteLine("============================");
        Console.WriteLine();

        // Validate assembly path
        if (!File.Exists(_assemblyPath))
        {
            throw new FileNotFoundException($"Assembly not found: {_assemblyPath}");
        }

        Console.WriteLine($"Assembly: {_assemblyPath}");
        Console.WriteLine($"Output: {_outputDirectory}");
        Console.WriteLine($"Base Namespace: {_baseNamespace}");
        Console.WriteLine($"Skip Segments: {_skipSegments}");
        Console.WriteLine();

        // Load assembly
        Console.WriteLine("Loading assembly...");
        var assembly = LoadAssembly(_assemblyPath);

        // Discover types
        Console.WriteLine("Discovering types...");
        var discovery = new TypeDiscovery(assembly);
        var discoveredTypes = discovery.DiscoverTypes();

        if (discoveredTypes.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: No types with [Command], [Query], or [ObservableQuery] attributes found.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"Found {discoveredTypes.Count} command/query types");
        Console.WriteLine();

        // Group by service
        var grouper = new ServiceGrouper();
        var serviceGroups = grouper.GroupByService(discoveredTypes);

        Console.WriteLine($"Grouped into {serviceGroups.Count} service(s):");
        foreach (var serviceName in serviceGroups.Keys)
        {
            Console.WriteLine($"  - {serviceName} ({serviceGroups[serviceName].Count} operations)");
        }
        Console.WriteLine();

        // Validate namespace consistency
        Console.WriteLine("Validating namespace consistency...");
        grouper.ValidateNamespaceConsistency(serviceGroups);
        Console.WriteLine("✓ All services have consistent namespaces");
        Console.WriteLine();

        // Generate code
        Console.WriteLine("Generating interfaces...");
        var codeGenerator = new CodeGenerator(_baseNamespace, _skipSegments);

        foreach (var (serviceName, types) in serviceGroups)
        {
            var serviceDefinition = codeGenerator.CreateServiceDefinition(serviceName, types);
            var code = codeGenerator.GenerateInterfaceCode(serviceDefinition);
            var outputPath = codeGenerator.GetOutputPath(serviceDefinition, _outputDirectory);

            var directory = Path.GetDirectoryName(outputPath)!;
            Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(outputPath, code);

            Console.WriteLine($"  ✓ Generated {Path.GetRelativePath(_outputDirectory, outputPath)}");
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ Successfully generated {serviceGroups.Count} service interface(s)");
        Console.ResetColor();
    }

    Assembly LoadAssembly(string assemblyPath)
    {
        var fullPath = Path.GetFullPath(assemblyPath);
        var loadContext = new IsolatedAssemblyLoadContext(fullPath);
        return loadContext.LoadFromAssemblyPath(fullPath);
    }
}
