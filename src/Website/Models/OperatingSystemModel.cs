// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing information about the operating system. This class cannot be inherited.
/// </summary>
/// <param name="description">The description of the operating system.</param>
/// <param name="architecture">The architecture of the operating system.</param>
/// <param name="version">The version of the operating system.</param>
/// <param name="is64Bit">Whether the operating system is 64-bit.</param>
public sealed class OperatingSystemModel(
    string description,
    string architecture,
    string version,
    bool is64Bit)
{
    /// <summary>
    /// Gets the description of the operating system.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets the architecture of the operating system.
    /// </summary>
    public string Architecture { get; } = architecture;

    /// <summary>
    /// Gets the version of the operating system.
    /// </summary>
    public string Version { get; } = version;

    /// <summary>
    /// Gets a value indicating whether the operating system is 64-bit.
    /// </summary>
    public bool Is64Bit { get; } = is64Bit;
}
