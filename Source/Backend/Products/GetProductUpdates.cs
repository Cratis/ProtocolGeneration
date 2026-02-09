// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Subjects;


namespace Backend.Products;

/// <summary>
/// Observable query to get product updates.
/// </summary>
[BelongsTo("Products")]
[ObservableQuery]
public class GetProductUpdates
{
    /// <summary>
    /// Handles the observable query execution.
    /// </summary>
    /// <returns>An observable stream of products.</returns>
    public ISubject<Product> Handle()
    {
        var subject = new Subject<Product>();

        Task.Run(async () =>
        {
            var random = new Random();
            while (true)
            {
                await Task.Delay(5000);
                var product = new Product(
                    ProductId.New(),
                    $"Product {random.Next(1, 100)}",
                    random.Next(10, 1000) + (decimal)random.NextDouble());
                subject.OnNext(product);
            }
        });

        return subject;
    }
}
