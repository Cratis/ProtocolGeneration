// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;
using Interfaces.Primitives;

namespace Interfaces.Products;
/// <summary>Data transfer object for CreateProduct.</summary>
[DataContract]
public class CreateProduct
{
    /// <summary>Gets or sets the Id.</summary>
[DataMember(Order = 1)]
    public Guid Id { get; set; }

    /// <summary>Gets or sets the Name.</summary>
[DataMember(Order = 2)]
    public string? Name { get; set; }

    /// <summary>Gets or sets the Price.</summary>
[DataMember(Order = 3)]
    public decimal Price { get; set; }

    /// <summary>Gets or sets the CreatedAt.</summary>
[DataMember(Order = 4)]
    public Interfaces.Primitives.SerializableDateTimeOffset? CreatedAt { get; set; }
}