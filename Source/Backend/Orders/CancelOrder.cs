// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Commands.ModelBound;

namespace Backend.Orders;

/// <summary>
/// Command to cancel an order.
/// </summary>
[Command]
public class CancelOrder
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public required OrderId Id { get; set; }

    /// <summary>
    /// Handles the command execution.
    /// </summary>
    public void Handle()
    {
        Console.WriteLine($"Canceling order: Id={Id.Value}");
    }
}
