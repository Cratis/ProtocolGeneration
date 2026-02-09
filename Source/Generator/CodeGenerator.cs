// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator;

/// <summary>
/// Generates C# interface code for services.
/// </summary>
class CodeGenerator
{
    readonly string _baseNamespace;
    readonly int _skipSegments;
    readonly TypeTransformer _typeTransformer;

    public CodeGenerator(string baseNamespace, int skipSegments)
    {
        _baseNamespace = baseNamespace;
        _skipSegments = skipSegments;
        _typeTransformer = new TypeTransformer();
    }

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
        var sourceNamespace = types.First().Namespace;
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
        var copyright = @"// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.";

        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
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
                            SyntaxFactory.Attribute(
                                SyntaxFactory.ParseName("OperationContract"),
                                SyntaxFactory.AttributeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.AttributeArgument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(operation.OperationNumber)))))))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            interfaceDeclaration = interfaceDeclaration.AddMembers(method);
        }

        namespaceDeclaration = namespaceDeclaration.AddMembers(interfaceDeclaration);

        var usingDirectives = new[]
        {
            "ProtoBuf.Grpc",
            "System.Collections.Generic",
            "System.Threading.Tasks"
        }.Select(ns => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns)));

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectives.ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        return copyright + "\n\n" + code;
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
}
