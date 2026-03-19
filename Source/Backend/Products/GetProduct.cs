// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;

namespace Backend.Products;

/// <summary>
/// Query to get a product by its identifier.
/// </summary>
[ReadModel]
public class GetProduct
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public required ProductId Id { get; set; }

    /// <summary>
    /// Handles the query execution.
    /// </summary>
    /// <returns>The product.</returns>
    public Product Handle()
    {
        return new Product(
            Id,
            "Sample Product",
            99.99m);
    }
}
