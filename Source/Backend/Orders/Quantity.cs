// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Backend.Orders;

/// <summary>
/// Represents the quantity of items in an order.
/// </summary>
/// <param name="Value">The underlying int value.</param>
public record Quantity(int Value) : ConceptAs<int>(Value)
{
    /// <summary>
    /// Gets a Quantity representing zero.
    /// </summary>
    public static readonly Quantity Zero = new(0);

    /// <summary>
    /// Implicitly converts an int to a Quantity.
    /// </summary>
    /// <param name="value">The int value.</param>
    public static implicit operator Quantity(int value) => new(value);
}
