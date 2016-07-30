// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A class containing Git metadata for the assembly. This class cannot be inherited.
    /// </summary>
    public static class GitMetadata
    {
        /// <summary>
        /// Gets the SHA for the Git branch the assembly was compiled from.
        /// </summary>
        public static string Branch { get; } = GetMetadataValue("CommitBranch", "Unknown");

        /// <summary>
        /// Gets the SHA for the Git commit the assembly was compiled from.
        /// </summary>
        public static string Commit { get; } = GetMetadataValue("CommitHash", "Local");

        /// <summary>
        /// Gets the Git commit SHA associated with this revision of the application.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the Git SHA-1 for the revision of the application.
        /// </returns>
        private static string GetMetadataValue(string name, string defaultValue)
        {
            return typeof(GitMetadata).GetTypeInfo().Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where((p) => string.Equals(p.Key, name, StringComparison.Ordinal))
                .Select((p) => p.Value)
                .DefaultIfEmpty(defaultValue)
                .FirstOrDefault();
        }
    }
}
