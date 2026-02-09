// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Transforms types from their source representation to their interface representation.
/// </summary>
class TypeTransformer
{
    public string TransformType(Type type, bool isReturnType = false)
    {
        // Handle ISubject<T> - extract T for IAsyncEnumerable
        if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("ISubject"))
        {
            var innerType = type.GetGenericArguments()[0];
            return TransformType(innerType, isReturnType);
        }

        // Handle ConceptAs<T> - unwrap to underlying type
        if (IsConceptAs(type))
        {
            var baseType = type.BaseType;
            if (baseType != null && baseType.IsGenericType)
            {
                var underlyingType = baseType.GetGenericArguments()[0];
                return TransformType(underlyingType, isReturnType);
            }
        }

        // Handle OneOf<T0, T1, ...>
        if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("OneOf"))
        {
            var genericArgs = type.GetGenericArguments();
            var transformedArgs = genericArgs.Select(t => TransformType(t, isReturnType));
            return $"Interfaces.Primitives.OneOf<{string.Join(", ", transformedArgs)}>";
        }

        // Handle DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            return "Interfaces.Primitives.SerializableDateTimeOffset";
        }

        // Handle nullable value types
        if (Nullable.GetUnderlyingType(type) != null)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return TransformType(underlyingType!, isReturnType) + "?";
        }

        // Handle generic types
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            var genericArgs = type.GetGenericArguments();
            var transformedArgs = genericArgs.Select(t => TransformType(t, isReturnType));

            var typeName = genericTypeDef.Name;
            var tickIndex = typeName.IndexOf('`');
            if (tickIndex > 0)
            {
                typeName = typeName.Substring(0, tickIndex);
            }

            return $"{typeName}<{string.Join(", ", transformedArgs)}>";
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            return TransformType(elementType, isReturnType) + "[]";
        }

        // Primitive types
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
        {
            return GetCSharpTypeName(type);
        }

        // Use fully qualified name for complex types
        return type.Name;
    }

    public string GetReturnType(Type commandOrQueryType, DiscoveredTypeKind kind)
    {
        if (kind == DiscoveredTypeKind.Command)
        {
            return "Task<CommandResult>";
        }

        // For queries and observable queries, look for a Handle method
        var handleMethod = commandOrQueryType.GetMethod("Handle");
        if (handleMethod == null)
        {
            throw new InvalidOperationException($"Type '{commandOrQueryType.FullName}' does not have a Handle method");
        }

        var returnType = handleMethod.ReturnType;

        if (kind == DiscoveredTypeKind.ObservableQuery)
        {
            // Extract the type from ISubject<T> and wrap in IAsyncEnumerable<T>
            var transformedType = TransformType(returnType, isReturnType: true);
            return $"IAsyncEnumerable<{transformedType}>";
        }

        // For regular queries, check if it's IEnumerable and extract element type
        if (returnType.IsGenericType)
        {
            var genericTypeDef = returnType.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(IEnumerable<>))
            {
                var elementType = returnType.GetGenericArguments()[0];
                var transformedElementType = TransformType(elementType, isReturnType: true);
                return $"Task<IEnumerable<{transformedElementType}>>";
            }
        }

        var transformedReturnType = TransformType(returnType, isReturnType: true);
        return $"Task<{transformedReturnType}>";
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

    string GetCSharpTypeName(Type type)
    {
        if (type == typeof(bool)) return "bool";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(sbyte)) return "sbyte";
        if (type == typeof(char)) return "char";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(int)) return "int";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(long)) return "long";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(short)) return "short";
        if (type == typeof(ushort)) return "ushort";
        if (type == typeof(string)) return "string";
        if (type == typeof(object)) return "object";

        return type.Name;
    }
}
