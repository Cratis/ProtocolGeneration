// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;

namespace Backend.Products;

/// <summary>
/// Query to get all products.
/// </summary>
[ReadModel]
public class GetProducts
{
    /// <summary>
    /// Handles the query execution.
    /// </summary>
    /// <returns>All products.</returns>
    public IEnumerable<Product> Handle()
    {
        return
        [
            new Product(ProductId.New(), "Laptop", 1299.99m),
            new Product(ProductId.New(), "Mouse", 29.99m),
            new Product(ProductId.New(), "Keyboard", 79.99m)
        ];
    }
}
