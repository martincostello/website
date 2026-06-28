// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Options;

namespace MartinCostello.Website.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="HttpRequest"/> class. This class cannot be inherited.
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// The query string used to append a version to content URLs. This field is read-only.
    /// </summary>
    private static readonly string VersionQuery = $"?v={GitMetadata.Commit}";

    /// <summary>
    /// Returns the canonical URI for the specified HTTP request with the optional path.
    /// </summary>
    /// <param name="request">The HTTP request to get the canonical URI from.</param>
    /// <param name="path">The optional path to get the canonical URI for.</param>
    /// <returns>
    /// The canonical URI to use for the specified HTTP request.
    /// </returns>
    public static string Canonical(this HttpRequest request, string? path = null)
    {
        var builder = new UriBuilder()
        {
            Host = request.Host.Host,
        };

        if (request.Host.Port is { } port)
        {
            builder.Port = port;
        }

        builder.Path = path ?? request.Path;
        builder.Query = string.Empty;
        builder.Scheme = "https";

#pragma warning disable CA1308 // Normalize strings to uppercase
        string canonicalUri = builder.Uri.AbsoluteUri.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

        if (!canonicalUri.EndsWith('/'))
        {
            canonicalUri += "/";
        }

        return canonicalUri;
    }

    /// <summary>
    /// Converts a virtual (relative) path to an CDN absolute URI, if configured.
    /// </summary>
    /// <param name="value">The <see cref="HttpRequest"/>.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <param name="options">The current site configuration.</param>
    /// <returns>The CDN absolute URI, if configured; otherwise the application absolute URI.</returns>
    public static string CdnContent(this HttpRequest value, string contentPath, SiteOptions options)
    {
        var cdn = options?.ExternalLinks?.Cdn;

        // Prefer empty images to a NullReferenceException
        if (cdn == null)
        {
            return string.Empty;
        }

        return $"{cdn}{value.Content(contentPath)?.TrimStart('/')}";
    }

    /// <summary>
    /// Converts a virtual (relative) path to a relative URI.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/>.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <param name="appendVersion">Whether to append a version to the URL.</param>
    /// <returns>The relatve URI to the content.</returns>
    public static string? Content(this HttpRequest request, string? contentPath, bool appendVersion = true)
    {
        string? result = string.Empty;

        if (!string.IsNullOrEmpty(contentPath))
        {
            if (contentPath[0] == '~')
            {
                var segment = new PathString(contentPath[1..]);
                var applicationPath = request.PathBase;

                var path = applicationPath.Add(segment);
                result = path.Value;
            }
            else
            {
                result = contentPath;
            }
        }

        if (appendVersion)
        {
            result += VersionQuery;
        }

        return result;
    }
}
