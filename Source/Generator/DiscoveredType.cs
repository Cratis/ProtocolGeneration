// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Represents a type that has been discovered with a command/query attribute.
/// </summary>
sealed class DiscoveredType
{
    public required Type Type { get; init; }
    public required string ServiceName { get; init; }
    public required DiscoveredTypeKind Kind { get; init; }
    public required string Namespace { get; init; }
}
