// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator;

/// <summary>
/// Generates Data Transfer Object (DTO) classes for commands, queries, and result types.
/// </summary>
class DtoGenerator
{
    readonly TypeTransformer _typeTransformer;
    readonly HashSet<Type> _generatedTypes = new();
    readonly HashSet<string> _excludedTypes = new()
    {
        "CommandResult",
        "Task",
        "IEnumerable",
        "IAsyncEnumerable",
        "SerializableDateTimeOffset",
        "OneOf"
    };

    public DtoGenerator()
    {
        _typeTransformer = new TypeTransformer();
    }

    public List<DtoDefinition> DiscoverDtosToGenerate(List<DiscoveredType> discoveredTypes)
    {
        var dtosToGenerate = new List<DtoDefinition>();
        var typesToProcess = new Queue<(Type type, string context)>();

        // Start with all command and query types
        foreach (var discoveredType in discoveredTypes)
        {
            typesToProcess.Enqueue((discoveredType.Type, $"Command/Query: {discoveredType.Type.Name}"));
            
            // Also add their return types
            if (discoveredType.Kind != DiscoveredTypeKind.Command)
            {
                var handleMethod = discoveredType.Type.GetMethod("Handle");
                if (handleMethod != null)
                {
                    var returnType = ExtractTypeFromTask(handleMethod.ReturnType);
                    if (returnType != null)
                    {
                        returnType = ExtractElementTypeFromEnumerable(returnType);
                        returnType = ExtractTypeFromSubject(returnType);
                        if (returnType != null)
                        {
                            typesToProcess.Enqueue((returnType, $"Result of {discoveredType.Type.Name}"));
                        }
                    }
                }
            }
        }

        // Process queue recursively
        while (typesToProcess.Count > 0)
        {
            var (type, context) = typesToProcess.Dequeue();

            if (ShouldGenerateDto(type))
            {
                var dto = CreateDtoDefinition(type);
                if (dto != null)
                {
                    dtosToGenerate.Add(dto);
                    _generatedTypes.Add(type);

                    // Add property types to queue
                    foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var propertyType = UnwrapType(property.PropertyType);
                        if (propertyType != null && ShouldGenerateDto(propertyType))
                        {
                            typesToProcess.Enqueue((propertyType, $"Property {property.Name} of {type.Name}"));
                        }
                    }
                }
            }
        }

        return dtosToGenerate;
    }

    public DtoDefinition UpdateDtoNamespace(DtoDefinition dto, string targetNamespace)
    {
        return new DtoDefinition
        {
            ClassName = dto.ClassName,
            Namespace = targetNamespace,
            SourceType = dto.SourceType,
            Properties = dto.Properties
        };
    }

    public string GenerateDtoCode(DtoDefinition dto)
    {
        var copyright = @"// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.";

        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(dto.Namespace));

        // Create class with DataContract attribute
        var classDeclaration = SyntaxFactory.ClassDeclaration(dto.ClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("DataContract")))));

        // Add XML documentation
        var xmlTrivia = SyntaxFactory.TriviaList(
            SyntaxFactory.Trivia(
                SyntaxFactory.DocumentationCommentTrivia(
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    SyntaxFactory.List(new XmlNodeSyntax[]
                    {
                        SyntaxFactory.XmlText("/// "),
                        SyntaxFactory.XmlElement(
                            SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("summary")),
                            SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("summary")))
                            .WithContent(
                                SyntaxFactory.List(new XmlNodeSyntax[]
                                {
                                    SyntaxFactory.XmlText($"Data transfer object for {dto.SourceType.Name}.")
                                })),
                        SyntaxFactory.XmlText("\n")
                    }))));

        classDeclaration = classDeclaration.WithLeadingTrivia(xmlTrivia);

        // Add properties
        var order = 1;
        foreach (var property in dto.Properties)
        {
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(property.TransformedTypeName),
                property.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(
                                SyntaxFactory.ParseName("DataMember"),
                                SyntaxFactory.AttributeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.AttributeArgument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal(order++)))
                                        .WithNameEquals(
                                            SyntaxFactory.NameEquals(
                                                SyntaxFactory.IdentifierName("Order")))))))))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            // Add XML documentation for property
            var propXmlTrivia = SyntaxFactory.TriviaList(
                SyntaxFactory.Trivia(
                    SyntaxFactory.DocumentationCommentTrivia(
                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                        SyntaxFactory.List(new XmlNodeSyntax[]
                        {
                            SyntaxFactory.XmlText("    /// "),
                            SyntaxFactory.XmlElement(
                                SyntaxFactory.XmlElementStartTag(SyntaxFactory.XmlName("summary")),
                                SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("summary")))
                                .WithContent(
                                    SyntaxFactory.List(new XmlNodeSyntax[]
                                    {
                                        SyntaxFactory.XmlText($"Gets or sets the {property.Name}.")
                                    })),
                            SyntaxFactory.XmlText("\n    ")
                        }))));

            propertyDeclaration = propertyDeclaration.WithLeadingTrivia(propXmlTrivia);
            classDeclaration = classDeclaration.AddMembers(propertyDeclaration);
        }

        namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

        // Add using directives
        var usingDirectives = new HashSet<string>
        {
            "System.Runtime.Serialization"
        };

        // Add conditional usings based on what types are used
        if (dto.Properties.Any(p => p.TransformedTypeName.Contains("SerializableDateTimeOffset") || p.TransformedTypeName.Contains("OneOf")))
        {
            usingDirectives.Add("Interfaces.Primitives");
        }

        if (dto.Properties.Any(p => p.TransformedTypeName.Contains("IEnumerable") || p.TransformedTypeName.Contains("List")))
        {
            usingDirectives.Add("System.Collections.Generic");
        }

        if (dto.Properties.Any(p => p.TransformedTypeName.Contains("Guid")))
        {
            usingDirectives.Add("System");
        }

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usingDirectives.OrderBy(ns => ns).Select(ns => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns))).ToArray())
            .AddMembers(namespaceDeclaration)
            .NormalizeWhitespace();

        var code = compilationUnit.ToFullString();

        return copyright + "\n\n" + code;
    }

    bool ShouldGenerateDto(Type type)
    {
        if (_generatedTypes.Contains(type))
        {
            return false;
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid))
        {
            return false;
        }

        if (type.IsEnum)
        {
            return false;
        }

        if (type.IsGenericParameter)
        {
            return false;
        }

        if (_excludedTypes.Contains(type.Name) || _excludedTypes.Any(excluded => type.Name.StartsWith(excluded)))
        {
            return false;
        }

        if (type.Namespace?.StartsWith("System") == true)
        {
            return false;
        }

        if (type.Namespace?.StartsWith("Microsoft") == true)
        {
            return false;
        }

        return true;
    }

    DtoDefinition? CreateDtoDefinition(Type type)
    {
        var properties = new List<DtoProperty>();
        
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip properties that return ISubject<T> - these are not for DTOs
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Name.StartsWith("ISubject"))
            {
                continue;
            }

            var transformedType = _typeTransformer.TransformType(property.PropertyType);
            
            properties.Add(new DtoProperty
            {
                Name = property.Name,
                OriginalType = property.PropertyType,
                TransformedTypeName = transformedType
            });
        }

        return new DtoDefinition
        {
            ClassName = type.Name,
            Namespace = type.Namespace ?? "Generated",
            SourceType = type,
            Properties = properties
        };
    }

    Type? UnwrapType(Type type)
    {
        // Unwrap ConceptAs<T>
        if (IsConceptAs(type))
        {
            var baseType = type.BaseType;
            if (baseType != null && baseType.IsGenericType)
            {
                return baseType.GetGenericArguments()[0];
            }
        }

        // Unwrap collections
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(IEnumerable<>) || 
                genericTypeDef == typeof(List<>) ||
                genericTypeDef == typeof(ICollection<>) ||
                genericTypeDef == typeof(IList<>))
            {
                return type.GetGenericArguments()[0];
            }

            // Unwrap OneOf<>
            if (genericTypeDef.Name.StartsWith("OneOf"))
            {
                // Process all generic arguments
                foreach (var arg in type.GetGenericArguments())
                {
                    var unwrapped = UnwrapType(arg);
                    if (unwrapped != null && ShouldGenerateDto(unwrapped))
                    {
                        return unwrapped;
                    }
                }
            }
        }

        // Unwrap arrays
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        return type;
    }

    Type? ExtractTypeFromTask(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return type.GetGenericArguments()[0];
        }
        return type == typeof(Task) ? null : type;
    }

    Type? ExtractElementTypeFromEnumerable(Type type)
    {
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(IEnumerable<>))
            {
                return type.GetGenericArguments()[0];
            }
        }
        return type;
    }

    Type? ExtractTypeFromSubject(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("ISubject"))
        {
            return type.GetGenericArguments()[0];
        }
        return type;
    }

    bool IsConceptAs(Type type)
    {
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition().Name == "ConceptAs`1")
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}

/// <summary>
/// Represents a DTO class to be generated.
/// </summary>
class DtoDefinition
{
    public required string ClassName { get; init; }
    public required string Namespace { get; init; }
    public required Type SourceType { get; init; }
    public required List<DtoProperty> Properties { get; init; }
}

/// <summary>
/// Represents a property in a DTO class.
/// </summary>
class DtoProperty
{
    public required string Name { get; init; }
    public required Type OriginalType { get; init; }
    public required string TransformedTypeName { get; init; }
}
