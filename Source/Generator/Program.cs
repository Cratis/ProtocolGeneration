// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine;
using Generator;

var rootCommand = new RootCommand("Protocol Interface Generator - Generates gRPC service interfaces from annotated types");

var assemblyOption = new Option<string>(
    name: "--assembly",
    description: "Path to the assembly to analyze")
{
    IsRequired = true
};

var outputOption = new Option<string>(
    name: "--output",
    description: "Output directory for generated interfaces")
{
    IsRequired = true
};

var baseNamespaceOption = new Option<string>(
    name: "--base-namespace",
    getDefaultValue: () => "Interfaces",
    description: "Base namespace for generated interfaces");

var skipSegmentsOption = new Option<int>(
    name: "--skip-segments",
    getDefaultValue: () => 1,
    description: "Number of namespace segments to skip from source types");

rootCommand.AddOption(assemblyOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(baseNamespaceOption);
rootCommand.AddOption(skipSegmentsOption);

rootCommand.SetHandler(HandleCommand, assemblyOption, outputOption, baseNamespaceOption, skipSegmentsOption);

return await rootCommand.InvokeAsync(args);

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
