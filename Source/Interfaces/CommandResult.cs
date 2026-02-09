// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;

namespace Interfaces;

/// <summary>
/// Represents the result of a command execution.
/// </summary>
[DataContract]
public class CommandResult
{
    /// <summary>
    /// Gets a successful command result.
    /// </summary>
    public static readonly CommandResult Success = new();

    /// <summary>
    /// Gets or sets a value indicating whether the command was successful.
    /// </summary>
    [DataMember(Order = 1)]
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message if the command failed.
    /// </summary>
    [DataMember(Order = 2)]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Creates a failed command result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed command result.</returns>
    public static CommandResult Failed(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
