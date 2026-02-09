// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator;

/// <summary>
/// Generates C# interface code for services.
/// </summary>
sealed class CodeGenerator(string baseNamespace, int skipSegments)
{
    static readonly string[] _defaultUsings =
    [
        "System.Collections.Generic",
        "System.ServiceModel",
        "System.Threading.Tasks"
    ];

    readonly string _baseNamespace = baseNamespace;
    readonly int _skipSegments = skipSegments;
    readonly TypeTransformer _typeTransformer = new();
    readonly DtoGenerator _dtoGenerator = new();

    public ServiceDefinition CreateServiceDefinition(string serviceName, List<DiscoveredType> types)
    {
        var operations = new List<ServiceOperation>();
        var operationNumber = 1;

        foreach (var type in types.OrderBy(t => t.Type.Name))
        {
            var methodName = type.Type.Name;
            var parameterType = type.Type.Name;
            var returnType = _typeTransformer.GetReturnType(type.Type, type.Kind);

            operations.Add(new ServiceOperation
            {
                MethodName = methodName,
                ParameterType = parameterType,
                ReturnType = returnType,
                OperationNumber = operationNumber++
            });
        }

        // Get namespace from the first type and apply skip segments
        var sourceNamespace = types[0].Namespace;
        var targetNamespace = ApplyNamespaceSkipping(sourceNamespace);

        return new ServiceDefinition
        {
            ServiceName = serviceName,
            Namespace = targetNamespace,
            Operations = operations
        };
    }

    public string GenerateInterfaceCode(ServiceDefinition service)
    {
        const string copyright = "// Copyright (c) Cratis. All rights reserved.\n" +
            "// Licensed under the MIT license. See LICENSE file in the project root for full license information.";

        var namespaceDeclaration = SyntaxFactory.FileScopedNamespaceDeclaration(
            SyntaxFactory.ParseName(service.Namespace));

        var interfaceName = $"I{service.ServiceName}Service";
        var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(interfaceName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("ServiceContract")))));

        foreach (var operation in service.Operations)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(operation.ReturnType),
                operation.MethodName)
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("command"))
                        .WithType(SyntaxFactory.ParseTypeName(operation.ParameterType)))
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("OperationContract")))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            interfaceDeclaration = interfaceDeclaration.AddMembers(method);
        }

        namespaceDeclaration = namespaceDeclaration.AddMembers(interfaceDeclaration);

        var usingDirectives = _defaultUsings
            .Select(ns => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns)));

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectives.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        return copyright + "\n\n" + code;
    }

    public string GetOutputPath(ServiceDefinition service, string outputDirectory)
    {
        var namespaceParts = service.Namespace.Split('.');
        var pathParts = namespaceParts.Skip(1).ToArray(); // Skip base namespace

        var directory = outputDirectory;
        if (pathParts.Length > 0)
        {
            directory = Path.Combine(outputDirectory, Path.Combine(pathParts));
        }

        var fileName = $"I{service.ServiceName}Service.cs";
        return Path.Combine(directory, fileName);
    }

    public List<DtoDefinition> DiscoverDtosForTypes(List<DiscoveredType> types)
    {
        return _dtoGenerator.DiscoverDtosToGenerate(types);
    }

    public string GenerateDtoCode(DtoDefinition dto)
    {
        return _dtoGenerator.GenerateDtoCode(dto);
    }

    public DtoDefinition UpdateDtoNamespace(DtoDefinition dto, string sourceNamespace)
    {
        var targetNamespace = ApplyNamespaceSkipping(sourceNamespace);
        return _dtoGenerator.UpdateDtoNamespace(dto, targetNamespace);
    }

    public string GetDtoOutputPath(DtoDefinition dto, string outputDirectory)
    {
        // Use the DTO's namespace to determine the path
        var namespaceParts = dto.Namespace.Split('.');
        var pathParts = namespaceParts.Skip(_skipSegments).ToArray();

        var directory = outputDirectory;
        if (pathParts.Length > 0)
        {
            directory = Path.Combine(outputDirectory, Path.Combine(pathParts));
        }

        var fileName = $"{dto.ClassName}.cs";
        return Path.Combine(directory, fileName);
    }

    string ApplyNamespaceSkipping(string sourceNamespace)
    {
        var segments = sourceNamespace.Split('.');
        var remainingSegments = segments.Skip(_skipSegments).ToArray();

        if (remainingSegments.Length == 0)
        {
            return _baseNamespace;
        }

        return $"{_baseNamespace}.{string.Join('.', remainingSegments)}";
    }
}
