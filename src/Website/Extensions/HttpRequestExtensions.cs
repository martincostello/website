// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using MartinCostello.Website.Options;

namespace MartinCostello.Website.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="HttpRequest"/> class. This class cannot be inherited.
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// The scheme prefix used for canonical URIs.
    /// </summary>
    private const string HttpsPrefix = "https://";

    /// <summary>
    /// The query string used to append a version to content URLs. This field is read-only.
    /// </summary>
    private static readonly string VersionQuery = $"?v={GitMetadata.Commit}";

    /// <summary>
    /// A cache of the relative URIs for content paths. This field is read-only.
    /// </summary>
    /// <remarks>
    /// The content paths are literals in the Razor slices and the path base does not vary
    /// between requests in practice, so the set of keys is small and bounded. Caching the
    /// values avoids allocating the same strings for every request that renders a page.
    /// </remarks>
    private static readonly ConcurrentDictionary<(string PathBase, string ContentPath, bool AppendVersion), string> ContentCache = new();

    /// <summary>
    /// A cache of the CDN URIs for content paths. This field is read-only.
    /// </summary>
    /// <remarks>
    /// See the remarks for <see cref="ContentCache"/>.
    /// </remarks>
    private static readonly ConcurrentDictionary<(string Cdn, string PathBase, string ContentPath), string> CdnContentCache = new();

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
        string host = request.Host.Value ?? string.Empty;
        string requestPath = path ?? request.Path;

        // The default HTTPS port is omitted from the canonical URI
        int hostLength = host.AsSpan().EndsWith(":443", StringComparison.Ordinal) ? host.Length - 4 : host.Length;

        bool prependSlash = requestPath.Length == 0 || requestPath[0] != '/';
        bool appendSlash = requestPath.Length > 0 && requestPath[^1] != '/';

        int length =
            HttpsPrefix.Length +
            hostLength +
            (prependSlash ? 1 : 0) +
            requestPath.Length +
            (appendSlash ? 1 : 0);

        return string.Create(length, (host, hostLength, requestPath, prependSlash, appendSlash), static (buffer, state) =>
        {
            (string host, int hostLength, string path, bool prependSlash, bool appendSlash) = state;

            HttpsPrefix.CopyTo(buffer);
            int written = HttpsPrefix.Length;

            written += host.AsSpan(0, hostLength).ToLowerInvariant(buffer[written..]);

            if (prependSlash)
            {
                buffer[written++] = '/';
            }

            written += path.AsSpan().ToLowerInvariant(buffer[written..]);

            if (appendSlash)
            {
                buffer[written] = '/';
            }
        });
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

        if (cdn == null)
        {
            return string.Empty;
        }

        return CdnContentCache.GetOrAdd(
            (cdn.ToString(), value.PathBase.Value ?? string.Empty, contentPath),
            static (key) =>
            {
                (string cdn, string requestPathBase, string contentPath) = key;

                ResolveContentPath(requestPathBase, contentPath, out var pathBase, out var path);

                pathBase = pathBase.TrimStart('/');

                if (pathBase.IsEmpty)
                {
                    path = path.TrimStart('/');
                }

                return string.Concat(cdn, pathBase, path, VersionQuery);
            });
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
        if (string.IsNullOrEmpty(contentPath))
        {
            return appendVersion ? VersionQuery : string.Empty;
        }

        if (contentPath[0] != '~' && !appendVersion)
        {
            return contentPath;
        }

        return ContentCache.GetOrAdd(
            (request.PathBase.Value ?? string.Empty, contentPath, appendVersion),
            static (key) =>
            {
                (string requestPathBase, string contentPath, bool appendVersion) = key;

                if (contentPath[0] != '~')
                {
                    return string.Concat(contentPath, VersionQuery);
                }

                ResolveContentPath(requestPathBase, contentPath, out var pathBase, out var path);

                return string.Concat(pathBase, path, appendVersion ? VersionQuery : []);
            });
    }

    /// <summary>
    /// Resolves the specified content path into the segments that make up its relative URI.
    /// </summary>
    /// <param name="requestPathBase">The path base of the HTTP request.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <param name="pathBase">When the method returns, contains the path base to prefix the path with, if any.</param>
    /// <param name="path">When the method returns, contains the path of the content.</param>
    private static void ResolveContentPath(
        string requestPathBase,
        string? contentPath,
        out ReadOnlySpan<char> pathBase,
        out ReadOnlySpan<char> path)
    {
        pathBase = default;
        path = contentPath;

        if (path.Length > 0 && path[0] == '~')
        {
            // A virtual path is relative to the application's path base
            pathBase = requestPathBase;
            path = path[1..];

            if (pathBase.EndsWith('/') && path.StartsWith('/'))
            {
                pathBase = pathBase[..^1];
            }
        }
    }
}
