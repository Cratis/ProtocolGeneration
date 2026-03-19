// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Backend.Products;
using Cratis.Arc.Queries.ModelBound;

namespace Backend.Orders;

/// <summary>
/// Query to get all orders.
/// </summary>
[ReadModel]
public class GetOrders
{
    /// <summary>
    /// Handles the query execution.
    /// </summary>
    /// <returns>All orders.</returns>
    public IEnumerable<Order> Handle()
    {
        return
        [
            new Order(OrderId.New(), ProductId.New(), 2),
            new Order(OrderId.New(), ProductId.New(), 1),
            new Order(OrderId.New(), ProductId.New(), 10)
        ];
    }
}
