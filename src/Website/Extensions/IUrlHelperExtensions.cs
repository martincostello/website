// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Extensions
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Options;

    /// <summary>
    /// A class containing extension methods for the <see cref=""/> class. This class cannot be inherited.
    /// </summary>
    public static class IUrlHelperExtensions
    {
        /// <summary>
        /// Converts a virtual (relative) path to an application absolute URI.
        /// </summary>
        /// <param name="value">The <see cref="IUrlHelper"/>.</param>
        /// <param name="contentPath">The virtual path of the content.</param>
        /// <returns>The application absolute URI.</returns>
        public static string AbsoluteContent(this IUrlHelper value, string contentPath)
        {
            var request = value.ActionContext.HttpContext.Request;
            return value.ToAbsolute(request.Host.Value, contentPath, forceHttps: false);
        }

        /// <summary>
        /// Converts a virtual (relative) path to an CDN absolute URI, if configured.
        /// </summary>
        /// <param name="value">The <see cref="IUrlHelper"/>.</param>
        /// <param name="contentPath">The virtual path of the content.</param>
        /// <param name="options">The current site configuration.</param>
        /// <param name="appendVersion">Whether to append a version query string parameter to the URL.</param>
        /// <returns>The CDN absolute URI, if configured; otherwise the application absolute URI..</returns>
        public static string CdnContent(this IUrlHelper value, string contentPath, SiteOptions options, bool appendVersion = true)
        {
            var cdn = options?.ExternalLinks?.Cdn;

            // Prefer empty images to a NullReferenceException
            if (cdn == null)
            {
                return string.Empty;
            }

            // Azure Blob storage is case-sensitive, so force all URLs to lowercase
#pragma warning disable CA1308 // Normalize strings to uppercase
            string url = value.ToAbsolute(cdn.Host, contentPath.ToLowerInvariant(), forceHttps: true);
#pragma warning restore CA1308 // Normalize strings to uppercase

            // asp-append-version="true" does not work for non-local resources
            if (appendVersion)
            {
                url += $"?v={GitMetadata.Commit}";
            }

            return url;
        }

        /// <summary>
        /// Converts a virtual (relative) path to an absolute URI.
        /// </summary>
        /// <param name="value">The <see cref="IUrlHelper"/>.</param>
        /// <param name="host">The hostname.</param>
        /// <param name="contentPath">The virtual path of the content.</param>
        /// <param name="forceHttps">Whether to force the use of HTTPS.</param>
        /// <returns>The application absolute URI.</returns>
        private static string ToAbsolute(this IUrlHelper value, string host, string contentPath, bool forceHttps)
        {
            var request = value.ActionContext.HttpContext.Request;
            var scheme = forceHttps ? "https" : request.Scheme;
            return new Uri(new Uri($"{scheme}://{host}"), value.Content(contentPath)).ToString();
        }
    }
}
