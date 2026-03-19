// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Commands.ModelBound;

namespace Backend.Products;

/// <summary>
/// Command to create a new product.
/// </summary>
[Command]
public class CreateProduct
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public required ProductId Id { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public required ProductName Name { get; set; }

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public required Price Price { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Handles the command execution.
    /// </summary>
    public void Handle()
    {
        Console.WriteLine($"Creating product: Id={Id.Value}, Name={Name.Value}, Price={Price.Value}, CreatedAt={CreatedAt}");
    }
}
