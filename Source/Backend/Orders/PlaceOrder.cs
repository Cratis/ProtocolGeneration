// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Backend.Products;

using OneOf;

namespace Backend.Orders;

/// <summary>
/// Command to place a new order.
/// </summary>
[BelongsTo("Orders")]
[Command]
public class PlaceOrder
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public OrderId Id { get; set; }

    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public ProductId ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity to order.
    /// </summary>
    public Quantity Quantity { get; set; }

    /// <summary>
    /// Gets or sets the delivery preference - either an address string or pickup location int.
    /// </summary>
    public OneOf<string, int> DeliveryPreference { get; set; }

    /// <summary>
    /// Handles the command execution.
    /// </summary>
    public void Handle()
    {
        var deliveryInfo = DeliveryPreference.Match(
            address => $"Delivery to address: {address}",
            pickupLocation => $"Pickup at location: {pickupLocation}");

        Console.WriteLine($"Placing order: Id={Id.Value}, ProductId={ProductId.Value}, Quantity={Quantity.Value}, {deliveryInfo}");
    }
}
