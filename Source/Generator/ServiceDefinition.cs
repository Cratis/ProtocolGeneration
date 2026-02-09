// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Represents a service with its operations.
/// </summary>
class ServiceDefinition
{
    public required string ServiceName { get; init; }
    public required string Namespace { get; init; }
    public required List<ServiceOperation> Operations { get; init; }
}

/// <summary>
/// Represents a single operation in a service.
/// </summary>
class ServiceOperation
{
    public required string MethodName { get; init; }
    public required string ParameterType { get; init; }
    public required string ReturnType { get; init; }
    public required int OperationNumber { get; init; }
}
