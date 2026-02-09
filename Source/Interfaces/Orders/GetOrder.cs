// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Interfaces.Orders
{
    /// <summary>Data transfer object for GetOrder.</summary>
[DataContract]
    public class GetOrder
    {
            /// <summary>Gets or sets the Id.</summary>
    [DataMember(Order = 1)]
        public Guid Id { get; set; }
    }
}