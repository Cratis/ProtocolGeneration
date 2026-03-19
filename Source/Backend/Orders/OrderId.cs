// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Backend.Orders;

/// <summary>
/// Represents the unique identifier for an order.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public record OrderId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Gets an OrderId representing an empty/not set value.
    /// </summary>
    public static readonly OrderId NotSet = new(Guid.Empty);

    /// <summary>
    /// Implicitly converts a Guid to an OrderId.
    /// </summary>
    /// <param name="value">The Guid value.</param>
    public static implicit operator OrderId(Guid value) => new(value);

    /// <summary>
    /// Creates a new OrderId with a unique value.
    /// </summary>
    /// <returns>A new OrderId.</returns>
    public static OrderId New() => new(Guid.NewGuid());
}
