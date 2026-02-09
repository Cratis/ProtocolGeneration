// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Backend.Products;


namespace Backend.Orders;

/// <summary>
/// Query to get an order by its identifier.
/// </summary>
[BelongsTo("Orders")]
[Query]
public class GetOrder
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public OrderId Id { get; set; }

    /// <summary>
    /// Handles the query execution.
    /// </summary>
    /// <returns>The order.</returns>
    public Order Handle()
    {
        return new Order(
            Id,
            ProductId.New(),
            5);
    }
}
