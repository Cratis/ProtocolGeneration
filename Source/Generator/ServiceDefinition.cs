// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Represents a service with its operations.
/// </summary>
sealed class ServiceDefinition
{
    public required string ServiceName { get; init; }
    public required string Namespace { get; init; }
    public required List<ServiceOperation> Operations { get; init; }
}
