// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// Represents a single operation in a service.
/// </summary>
sealed class ServiceOperation
{
    public required string MethodName { get; init; }
    public required string ParameterType { get; init; }
    public required string ReturnType { get; init; }
    public required int OperationNumber { get; init; }
}
