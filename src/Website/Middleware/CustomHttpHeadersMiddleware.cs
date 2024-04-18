// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text;
using MartinCostello.Website.Extensions;
using MartinCostello.Website.Options;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MartinCostello.Website.Middleware;

/// <summary>
/// A class representing middleware for adding custom HTTP response headers. This class cannot be inherited.
/// </summary>
public sealed class CustomHttpHeadersMiddleware
{
    /// <summary>
    /// The delegate for the next part of the pipeline. This field is read-only.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The <see cref="IConfiguration"/> to use. This field is read-only.
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The current <c>Content-Security-Policy</c> HTTP response header value. This field is read-only.
    /// </summary>
    private readonly string _contentSecurityPolicy;

    /// <summary>
    /// The current <c>Content-Security-Policy-Report-Only</c> HTTP response header value. This field is read-only.
    /// </summary>
    private readonly string _contentSecurityPolicyReportOnly;

    /// <summary>
    /// The current <c>Expect-CT</c> HTTP response header value. This field is read-only.
    /// </summary>
    private readonly string _expectCTValue;

    /// <summary>
    /// The name of the current hosting environment. This field is read-only.
    /// </summary>
    private readonly string _environmentName;

    /// <summary>
    /// Whether the current hosting environment is production. This field is read-only.
    /// </summary>
    private readonly bool _isProduction;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomHttpHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The delegate for the next part of the pipeline.</param>
    /// <param name="environment">The current hosting environment.</param>
    /// <param name="config">The current configuration.</param>
    /// <param name="options">The current site configuration options.</param>
    public CustomHttpHeadersMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        IConfiguration config,
        IOptions<SiteOptions> options)
    {
        _next = next;
        _config = config;

        _isProduction = environment.IsProduction();
        _environmentName = config.AzureEnvironment();

        _contentSecurityPolicy = BuildContentSecurityPolicy(_isProduction, false, options.Value);
        _contentSecurityPolicyReportOnly = BuildContentSecurityPolicy(_isProduction, true, options.Value);

        _expectCTValue = BuildExpectCT(options.Value);
    }

    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the actions performed by the middleware.
    /// </returns>
    public Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
            {
                context.Response.Headers.Remove(HeaderNames.Server);
                context.Response.Headers.Remove(HeaderNames.XPoweredBy);

                context.Response.Headers.ContentSecurityPolicy = _contentSecurityPolicy;
                context.Response.Headers.ContentSecurityPolicyReportOnly = _contentSecurityPolicyReportOnly;
                context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
                context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.Append("X-Download-Options", "noopen");
                context.Response.Headers.XFrameOptions = "DENY";
                context.Response.Headers.XXSSProtection = "1; mode=block";

                if (context.Request.IsHttps)
                {
                    context.Response.Headers.Append("Expect-CT", _expectCTValue);
                }

                context.Response.Headers.Append("X-Datacenter", _config.AzureDatacenter());

#if DEBUG
                context.Response.Headers.Append("X-Debug", "true");
#endif

                if (_environmentName is not null)
                {
                    context.Response.Headers.Append("X-Environment", _environmentName);
                }

                context.Response.Headers.Append("X-Instance", Environment.MachineName);
                context.Response.Headers.Append("X-Request-Id", context.TraceIdentifier);
                context.Response.Headers.Append("X-Revision", GitMetadata.Commit);

                return Task.CompletedTask;
            });

        return _next(context);
    }

    /// <summary>
    /// Builds the Content Security Policy to use for the website.
    /// </summary>
    /// <param name="isProduction">Whether the current environment is production.</param>
    /// <param name="isReport">Whether the policy is being generated for the report.</param>
    /// <param name="options">The current site configuration options.</param>
    /// <returns>
    /// A <see cref="string"/> containing the Content Security Policy to use.
    /// </returns>
    private static string BuildContentSecurityPolicy(bool isProduction, bool isReport, SiteOptions options)
    {
        var cdn = GetCdnOriginForContentSecurityPolicy(options);

        var policies = new Dictionary<string, IList<string>>()
        {
            ["default-src"] = [Csp.Self, Csp.Data],
            ["script-src"] = [Csp.Self, Csp.Inline],
            ["style-src"] = [Csp.Self, Csp.Inline],
            ["img-src"] = [Csp.Self, Csp.Data, cdn],
            ["font-src"] = [Csp.Self],
            ["connect-src"] = [Csp.Self, GetApiOriginForContentSecurityPolicy(options)],
            ["media-src"] = [Csp.None],
            ["object-src"] = [],
            ["child-src"] = [Csp.Self],
            ["frame-ancestors"] = [Csp.None],
            ["form-action"] = [Csp.Self],
            ["block-all-mixed-content"] = [],
            ["base-uri"] = [Csp.Self],
            ["manifest-src"] = [Csp.Self],
            ["worker-src"] = [Csp.Self],
        };

        var builder = new StringBuilder();

        foreach (var pair in policies)
        {
            builder.Append(pair.Key);

            IList<string> origins = pair.Value;

            if (options?.ContentSecurityPolicyOrigins != null &&
                options.ContentSecurityPolicyOrigins.TryGetValue(pair.Key, out IList<string>? configOrigins))
            {
                origins = [.. origins.Concat(configOrigins)];
            }

            origins = origins
                .Where((p) => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            if (origins.Count > 0)
            {
                builder.Append(' ');
                builder.Append(string.Join(" ", origins));
            }

            builder.Append(';');
        }

        if (!isReport && isProduction)
        {
            builder.Append("upgrade-insecure-requests;");
        }

        if (options?.ExternalLinks?.Reports?.ContentSecurityPolicy != null)
        {
            builder.Append(CultureInfo.InvariantCulture, $"report-uri {options.ExternalLinks.Reports.ContentSecurityPolicy};");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the value to use for the <c>Expect-CT</c> HTTP response header.
    /// </summary>
    /// <param name="options">The current site configuration options.</param>
    /// <returns>
    /// A <see cref="string"/> containing the <c>Expect-CT</c> value to use.
    /// </returns>
    private static string BuildExpectCT(SiteOptions options)
    {
        var builder = new StringBuilder();

        bool enforce = options?.CertificateTransparency?.Enforce == true;

        if (enforce)
        {
            builder.Append("enforce; ");
        }

        builder.AppendFormat(
            CultureInfo.InvariantCulture,
            "max-age={0};",
            (int)(options?.CertificateTransparency?.MaxAge.TotalSeconds ?? 0));

        if (enforce)
        {
            if (options?.ExternalLinks?.Reports?.ExpectCTEnforce != null)
            {
                builder.Append(CultureInfo.InvariantCulture, $" report-uri {options.ExternalLinks.Reports.ExpectCTEnforce}");
            }
        }
        else
        {
            if (options?.ExternalLinks?.Reports?.ExpectCTReportOnly != null)
            {
                builder.Append(CultureInfo.InvariantCulture, $" report-uri {options.ExternalLinks.Reports.ExpectCTReportOnly}");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the API origin to use for the Content Security Policy.
    /// </summary>
    /// <param name="options">The current site options.</param>
    /// <returns>
    /// The origin to use for the API, if any.
    /// </returns>
    private static string GetApiOriginForContentSecurityPolicy(SiteOptions options)
    {
        return options?.ExternalLinks?.Api?.IsAbsoluteUri is true ?
            GetOriginForContentSecurityPolicy(options?.ExternalLinks?.Api) :
            string.Empty;
    }

    /// <summary>
    /// Gets the CDN origin to use for the Content Security Policy.
    /// </summary>
    /// <param name="options">The current site options.</param>
    /// <returns>
    /// The origin to use for the CDN, if any.
    /// </returns>
    private static string GetCdnOriginForContentSecurityPolicy(SiteOptions options)
    {
        return GetOriginForContentSecurityPolicy(options?.ExternalLinks?.Cdn);
    }

    /// <summary>
    /// Gets the origin to use for the Content Security Policy from the specified URI.
    /// </summary>
    /// <param name="baseUri">The base URI to get the origin for.</param>
    /// <returns>
    /// The origin to use for the URI, if any.
    /// </returns>
    private static string GetOriginForContentSecurityPolicy(Uri? baseUri)
    {
        if (baseUri == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(baseUri.Host);

        if (!baseUri.IsDefaultPort)
        {
            builder.Append(CultureInfo.InvariantCulture, $":{baseUri.Port}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// A class containing Content Security Policy constants.
    /// </summary>
    private static class Csp
    {
        /// <summary>
        /// The origin for a data URI.
        /// </summary>
        internal const string Data = "data:";

        /// <summary>
        /// The directive to allow inline content.
        /// </summary>
        internal const string Inline = "'unsafe-inline'";

        /// <summary>
        /// The directive to allow no origins.
        /// </summary>
        internal const string None = "'none'";

        /// <summary>
        /// The origin for the site itself.
        /// </summary>
        internal const string Self = "'self'";
    }
}
