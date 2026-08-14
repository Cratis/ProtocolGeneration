// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using Generator;

var rootCommand = new RootCommand("Protocol Interface Generator - Generates gRPC service interfaces from annotated types");

var assemblyOption = new Option<string>("--assembly")
{
    Description = "Path to the assembly to analyze",
    Required = true
};

var outputOption = new Option<string>("--output")
{
    Description = "Output directory for generated interfaces",
    Required = true
};

var baseNamespaceOption = new Option<string>("--base-namespace")
{
    Description = "Base namespace for generated interfaces",
    DefaultValueFactory = _ => "Interfaces"
};

var skipSegmentsOption = new Option<int>("--skip-segments")
{
    Description = "Number of namespace segments to skip from source types",
    DefaultValueFactory = _ => 1
};

rootCommand.Options.Add(assemblyOption);
rootCommand.Options.Add(outputOption);
rootCommand.Options.Add(baseNamespaceOption);
rootCommand.Options.Add(skipSegmentsOption);

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    var assembly = parseResult.GetValue(assemblyOption)!;
    var output = parseResult.GetValue(outputOption)!;
    var baseNamespace = parseResult.GetValue(baseNamespaceOption) ?? "Interfaces";
    var skipSegments = parseResult.GetValue(skipSegmentsOption);

    await HandleCommand(assembly, output, baseNamespace, skipSegments);
});

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();

static async Task HandleCommand(string assembly, string output, string baseNamespace, int skipSegments)
{
    try
    {
        var generator = new InterfaceGenerator(assembly, output, baseNamespace, skipSegments);
        await generator.GenerateAsync();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        await Console.Error.WriteLineAsync($"Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}
