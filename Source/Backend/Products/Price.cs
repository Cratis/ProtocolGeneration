// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Backend.Products;

/// <summary>
/// Represents the price of a product.
/// </summary>
/// <param name="Value">The underlying decimal value.</param>
public record Price(decimal Value) : ConceptAs<decimal>(Value)
{
    /// <summary>
    /// Gets a Price representing zero.
    /// </summary>
    public static readonly Price Zero = new(0m);

    /// <summary>
    /// Implicitly converts a decimal to a Price.
    /// </summary>
    /// <param name="value">The decimal value.</param>
    public static implicit operator Price(decimal value) => new(value);
}
