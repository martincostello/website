// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the versions of the .NET components in use. This class cannot be inherited.
/// </summary>
/// <param name="runtime">The version of the .NET runtime.</param>
/// <param name="aspNetCore">The version of ASP.NET Core.</param>
public sealed class DotnetVersionsModel(string runtime, string aspNetCore)
{
    /// <summary>
    /// Gets the version of the .NET runtime.
    /// </summary>
    public string Runtime { get; } = runtime;

    /// <summary>
    /// Gets the version of ASP.NET Core.
    /// </summary>
    public string AspNetCore { get; } = aspNetCore;
}
