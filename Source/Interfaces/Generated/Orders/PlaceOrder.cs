// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Interfaces.Primitives;
using System;
using System.Runtime.Serialization;

namespace Interfaces.Orders
{
    /// <summary>Data transfer object for PlaceOrder.</summary>
[DataContract]
    public class PlaceOrder
    {
            /// <summary>Gets or sets the Id.</summary>
    [DataMember(Order = 1)]
        public Guid Id { get; set; }

            /// <summary>Gets or sets the ProductId.</summary>
    [DataMember(Order = 2)]
        public Guid ProductId { get; set; }

            /// <summary>Gets or sets the Quantity.</summary>
    [DataMember(Order = 3)]
        public int Quantity { get; set; }

            /// <summary>Gets or sets the DeliveryPreference.</summary>
    [DataMember(Order = 4)]
        public Interfaces.Primitives.OneOf<string, int> DeliveryPreference { get; set; }
    }
}