// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Backend.Products;

/// <summary>
/// Represents the unique identifier for a product.
/// </summary>
/// <param name="Value">The underlying Guid value.</param>
public record ProductId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Gets a ProductId representing an empty/not set value.
    /// </summary>
    public static readonly ProductId NotSet = new(Guid.Empty);

    /// <summary>
    /// Implicitly converts a Guid to a ProductId.
    /// </summary>
    /// <param name="value">The Guid value.</param>
    public static implicit operator ProductId(Guid value) => new(value);

    /// <summary>
    /// Creates a new ProductId with a unique value.
    /// </summary>
    /// <returns>A new ProductId.</returns>
    public static ProductId New() => new(Guid.NewGuid());
}
