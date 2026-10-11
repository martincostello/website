// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the version information for the application. This class cannot be inherited.
/// </summary>
/// <param name="applicationVersion">The version of the application.</param>
/// <param name="frameworkDescription">The description of the .NET framework in use.</param>
/// <param name="operatingSystem">Information about the operating system.</param>
/// <param name="process">Information about the current process.</param>
/// <param name="dotnetVersions">The versions of the .NET components in use.</param>
public sealed class VersionModel(
    string applicationVersion,
    string frameworkDescription,
    OperatingSystemModel operatingSystem,
    ProcessModel process,
    DotnetVersionsModel dotnetVersions)
{
    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    public string ApplicationVersion { get; } = applicationVersion;

    /// <summary>
    /// Gets the description of the .NET framework in use.
    /// </summary>
    public string FrameworkDescription { get; } = frameworkDescription;

    /// <summary>
    /// Gets information about the operating system.
    /// </summary>
    public OperatingSystemModel OperatingSystem { get; } = operatingSystem;

    /// <summary>
    /// Gets information about the current process.
    /// </summary>
    public ProcessModel Process { get; } = process;

    /// <summary>
    /// Gets the versions of the .NET components in use.
    /// </summary>
    public DotnetVersionsModel DotnetVersions { get; } = dotnetVersions;
}
