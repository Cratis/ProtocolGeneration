// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Commands.ModelBound;

namespace Backend.Products;

/// <summary>
/// Command to update a product's price.
/// </summary>
[Command]
public class UpdateProductPrice
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public required ProductId Id { get; set; }

    /// <summary>
    /// Gets or sets the new price.
    /// </summary>
    public required Price Price { get; set; }

    /// <summary>
    /// Handles the command execution.
    /// </summary>
    public void Handle()
    {
        Console.WriteLine($"Updating product price: Id={Id.Value}, NewPrice={Price.Value}");
    }
}
