// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Represents a property in a DTO class.
/// </summary>
sealed class DtoProperty
{
    public required string Name { get; init; }
    public required Type OriginalType { get; init; }
    public required string TransformedTypeName { get; init; }
}
