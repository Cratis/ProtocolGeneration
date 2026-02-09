// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Backend.Products;

namespace Backend.Orders;

/// <summary>
/// Represents an order.
/// </summary>
/// <param name="Id">The order identifier.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="Quantity">The quantity ordered.</param>
public record Order(OrderId Id, ProductId ProductId, Quantity Quantity);
