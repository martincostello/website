// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing information about the current process. This class cannot be inherited.
/// </summary>
/// <param name="architecture">The architecture of the process.</param>
/// <param name="is64BitProcess">Whether the process is 64-bit.</param>
/// <param name="isNativeAoT">Whether the process is compiled using Native AoT.</param>
/// <param name="isPrivilegedProcess">Whether the process is privileged.</param>
/// <param name="uptime">The uptime of the process.</param>
public sealed class ProcessModel(
    string architecture,
    bool is64BitProcess,
    bool isNativeAoT,
    bool isPrivilegedProcess,
    string uptime)
{
    /// <summary>
    /// Gets the architecture of the process.
    /// </summary>
    public string Architecture { get; } = architecture;

    /// <summary>
    /// Gets a value indicating whether the process is 64-bit.
    /// </summary>
    public bool Is64BitProcess { get; } = is64BitProcess;

    /// <summary>
    /// Gets a value indicating whether the process is compiled using Native AoT.
    /// </summary>
    public bool IsNativeAoT { get; } = isNativeAoT;

    /// <summary>
    /// Gets a value indicating whether the process is privileged.
    /// </summary>
    public bool IsPrivilegedProcess { get; } = isPrivilegedProcess;

    /// <summary>
    /// Gets the uptime of the process.
    /// </summary>
    public string Uptime { get; } = uptime;
}
