// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Represents a DTO class to be generated.
/// </summary>
sealed class DtoDefinition
{
    public required string ClassName { get; init; }
    public required string Namespace { get; init; }
    public required Type SourceType { get; init; }
    public required List<DtoProperty> Properties { get; init; }
}
