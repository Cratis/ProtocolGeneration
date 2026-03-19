// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace Server;

/// <summary>
/// Maps between DTO types and backend types.
/// </summary>
public static class TypeMapper
{
    /// <summary>
    /// Maps a DTO object to a backend type.
    /// </summary>
    /// <param name="source">The source DTO object.</param>
    /// <param name="targetType">The target backend type.</param>
    /// <returns>The mapped backend object.</returns>
    public static object MapToBackend(object source, Type targetType)
    {
        if (source == null) return null!;

        var sourceType = source.GetType();

        // Handle direct assignable types
        if (targetType.IsAssignableFrom(sourceType))
        {
            return source;
        }

        // Handle SerializableDateTimeOffset -> DateTimeOffset
        if (sourceType.FullName == "Interfaces.Primitives.SerializableDateTimeOffset" && targetType == typeof(DateTimeOffset))
        {
            var implicitOp = sourceType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, [sourceType], null);
            if (implicitOp != null && implicitOp.ReturnType == typeof(DateTimeOffset))
            {
                return implicitOp.Invoke(null, [source])!;
            }
        }

        // Handle Interfaces.Primitives.OneOf<T0,T1> -> OneOf.OneOf<T0,T1>
        if (sourceType.Namespace == "Interfaces.Primitives" && sourceType.Name.StartsWith("OneOf`"))
        {
            return MapOneOfToBackend(source, targetType);
        }

        // Handle ConceptAs types (Guid -> ProductId, etc.)
        if (IsConceptAsType(targetType))
        {
            var underlyingType = targetType.BaseType?.GetGenericArguments().FirstOrDefault();
            if (underlyingType != null)
            {
                // Try implicit conversion operator
                var implicitOp = targetType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, [underlyingType], null);
                if (implicitOp != null)
                {
                    return implicitOp.Invoke(null, [source])!;
                }

                // Try constructor
                var ctor = targetType.GetConstructor([underlyingType]);
                if (ctor != null)
                {
                    return ctor.Invoke([source]);
                }
            }
        }

        // Handle complex object mapping
        var target = Activator.CreateInstance(targetType)!;
        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(p => p.Name == sourceProp.Name && p.CanWrite);
            if (targetProp != null)
            {
                var sourceValue = sourceProp.GetValue(source);
                if (sourceValue != null)
                {
                    var mappedValue = MapToBackend(sourceValue, targetProp.PropertyType);
                    targetProp.SetValue(target, mappedValue);
                }
            }
        }

        return target;
    }

    /// <summary>
    /// Maps a backend object to a DTO type.
    /// </summary>
    /// <param name="source">The source backend object.</param>
    /// <param name="targetType">The target DTO type.</param>
    /// <returns>The mapped DTO object.</returns>
    public static object MapToDto(object source, Type targetType)
    {
        if (source == null) return null!;

        var sourceType = source.GetType();

        // Handle direct assignable types
        if (targetType.IsAssignableFrom(sourceType))
        {
            return source;
        }

        // Handle DateTimeOffset -> SerializableDateTimeOffset
        if (sourceType == typeof(DateTimeOffset) && targetType.FullName == "Interfaces.Primitives.SerializableDateTimeOffset")
        {
            var implicitOp = targetType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, [typeof(DateTimeOffset)], null);
            if (implicitOp != null)
            {
                return implicitOp.Invoke(null, [source])!;
            }
        }

        // Handle OneOf.OneOf<T0,T1> -> Interfaces.Primitives.OneOf<T0,T1>
        if (sourceType.Namespace == "OneOf" && sourceType.Name.StartsWith("OneOf`"))
        {
            return MapOneOfToDto(source, targetType);
        }

        // Handle ConceptAs types (ProductId -> Guid, etc.)
        if (IsConceptAsType(sourceType))
        {
            var valueProperty = sourceType.GetProperty("Value");
            if (valueProperty != null)
            {
                return valueProperty.GetValue(source)!;
            }
        }

        // Handle collections
        if (IsEnumerableType(sourceType) && IsEnumerableType(targetType))
        {
            var sourceElementType = GetEnumerableElementType(sourceType);
            var targetElementType = GetEnumerableElementType(targetType);

            if (sourceElementType != null && targetElementType != null)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetElementType))!;
                foreach (var item in (IEnumerable)source)
                {
                    list.Add(MapToDto(item, targetElementType));
                }
                return list;
            }
        }

        // Handle complex object mapping
        var target = Activator.CreateInstance(targetType)!;
        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(p => p.Name == sourceProp.Name && p.CanWrite);
            if (targetProp != null)
            {
                var sourceValue = sourceProp.GetValue(source);
                if (sourceValue != null)
                {
                    var mappedValue = MapToDto(sourceValue, targetProp.PropertyType);
                    targetProp.SetValue(target, mappedValue);
                }
            }
        }

        return target;
    }

    static bool IsConceptAsType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name == "ConceptAs`1")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    static bool IsEnumerableType(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    static Type? GetEnumerableElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                return genericArgs[0];
            }
        }

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments().FirstOrDefault();
    }

    static object MapOneOfToBackend(object source, Type targetType)
    {
        var sourceType = source.GetType();
        var sourceGenericArgs = sourceType.GetGenericArguments();
        var targetGenericArgs = targetType.GetGenericArguments();

        if (sourceGenericArgs.Length != targetGenericArgs.Length)
        {
            throw new InvalidOperationException("OneOf type mismatch");
        }

        // Get Value0, Value1, etc. properties from source
        for (var i = 0; i < sourceGenericArgs.Length; i++)
        {
            var valueProp = sourceType.GetProperty($"Value{i}");
            if (valueProp != null)
            {
                var value = valueProp.GetValue(source);
                if (value != null)
                {
                    // Create target OneOf with the appropriate constructor
                    var ctor = targetType.GetConstructor([targetGenericArgs[i]]);
                    if (ctor != null)
                    {
                        return ctor.Invoke([value]);
                    }
                }
            }
        }

        // Default constructor if no value is set
        return Activator.CreateInstance(targetType)!;
    }

    static object MapOneOfToDto(object source, Type targetType)
    {
        var sourceType = source.GetType();
        var sourceGenericArgs = sourceType.GetGenericArguments();
        var targetGenericArgs = targetType.GetGenericArguments();

        if (sourceGenericArgs.Length != targetGenericArgs.Length)
        {
            throw new InvalidOperationException("OneOf type mismatch");
        }

        // Get value from source using reflection
        // OneOf library has IsT0, IsT1, AsT0, AsT1 properties
        for (var i = 0; i < sourceGenericArgs.Length; i++)
        {
            var isTProp = sourceType.GetProperty($"IsT{i}");
            if (isTProp != null)
            {
                var isSet = (bool)(isTProp.GetValue(source) ?? false);
                if (isSet)
                {
                    var asTProp = sourceType.GetProperty($"AsT{i}");
                    if (asTProp != null)
                    {
                        var value = asTProp.GetValue(source);
                        if (value != null)
                        {
                            // Create target Interfaces.Primitives.OneOf with the appropriate constructor
                            var ctor = targetType.GetConstructor([targetGenericArgs[i]]);
                            if (ctor != null)
                            {
                                return ctor.Invoke([value]);
                            }
                        }
                    }
                }
            }
        }

        // Default constructor if no value is set
        return Activator.CreateInstance(targetType)!;
    }
}
