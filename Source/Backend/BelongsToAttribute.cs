// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Backend;

/// <summary>
/// Attribute to indicate which service a command or query belongs to.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BelongsToAttribute"/> class.
/// </remarks>
/// <param name="serviceName">The name of the service.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BelongsToAttribute(string serviceName) : Attribute
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string ServiceName { get; } = serviceName;
}
