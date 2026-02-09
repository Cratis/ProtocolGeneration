// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Backend;

/// <summary>
/// Marks a class as a query for code generation purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class QueryAttribute : Attribute
{
}
