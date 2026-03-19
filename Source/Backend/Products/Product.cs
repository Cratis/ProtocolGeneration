// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Backend.Products;

/// <summary>
/// Represents a product.
/// </summary>
/// <param name="Id">The product identifier.</param>
/// <param name="Name">The product name.</param>
/// <param name="Price">The product price.</param>
public record Product(ProductId Id, ProductName Name, Price Price);
