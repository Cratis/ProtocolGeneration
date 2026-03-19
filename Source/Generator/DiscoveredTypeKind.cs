// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Generator;

/// <summary>
/// The kind of discovered type.
/// </summary>
enum DiscoveredTypeKind
{
    Command = 0,
    Query = 1,
    ObservableQuery = 2
}
