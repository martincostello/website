// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website;

/// <summary>
/// A class containing metadata about the application that is constant for the lifetime of the process. This class cannot be inherited.
/// </summary>
internal static class ApplicationMetadata
{
    /// <summary>
    /// Gets the formatted timestamp the application was built at.
    /// </summary>
    public static string BuildTimestamp { get; } = GitMetadata.Timestamp.ToString("u", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the name of the machine the application is running on.
    /// </summary>
    /// <remarks>
    /// <see cref="Environment.MachineName"/> is not cached by the runtime and allocates a new string on each access.
    /// </remarks>
    public static string MachineName { get; } = Environment.MachineName;

    /// <summary>
    /// Gets the abbreviated Git commit SHA the application was built from.
    /// </summary>
    public static string ShortCommit { get; } = GitMetadata.Commit[..Math.Min(7, GitMetadata.Commit.Length)];
}
