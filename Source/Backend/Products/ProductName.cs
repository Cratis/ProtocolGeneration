// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Backend.Products;

/// <summary>
/// Represents the name of a product.
/// </summary>
/// <param name="Value">The underlying string value.</param>
public record ProductName(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Gets a ProductName representing an empty/not set value.
    /// </summary>
    public static readonly ProductName Empty = new(string.Empty);

    /// <summary>
    /// Implicitly converts a string to a ProductName.
    /// </summary>
    /// <param name="value">The string value.</param>
    public static implicit operator ProductName(string value) => new(value);
}
