// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using Backend.Products;
using Cratis.Arc.Queries.ModelBound;

namespace Backend.Orders;

/// <summary>
/// Observable query to get order updates.
/// </summary>
[ReadModel]
public class GetOrderUpdates
{
    /// <summary>
    /// Handles the observable query execution.
    /// </summary>
    /// <returns>An observable stream of orders.</returns>
    public ISubject<Order> Handle()
    {
        var subject = new Subject<Order>();

        Task.Run(async () =>
        {
            var random = new Random();
            while (true)
            {
                await Task.Delay(3000);
                var order = new Order(
                    OrderId.New(),
                    ProductId.New(),
                    random.Next(1, 20));
                subject.OnNext(order);
            }
        });

        return subject;
    }
}
