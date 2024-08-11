// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using MartinCostello.Website.Middleware;
using MartinCostello.Website.Options;
using MartinCostello.Website.Slices;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;

namespace MartinCostello.Website;

/// <summary>
/// Extension methods for configuring the website application.
/// </summary>
public static class WebsiteBuilder
{
    /// <summary>
    /// Adds website services to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>
    /// The value passed by <paramref name="builder"/> for chaining.
    /// </returns>
    public static WebApplicationBuilder AddWebsite(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();

        builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
        builder.Services.ConfigureHttpJsonOptions((options) =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.TypeInfoResolverChain.Add(ApplicationJsonSerializerContext.Default);
        });

        builder.Services.Configure<StaticFileOptions>((options) =>
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";

            options.ContentTypeProvider = provider;
            options.DefaultContentType = "application/json";
            options.ServeUnknownFileTypes = true;

            options.OnPrepareResponse = (context) =>
            {
                var maxAge = TimeSpan.FromDays(7);

                if (context.File.Exists && builder.Environment.IsProduction())
                {
                    string? extension = Path.GetExtension(context.File.PhysicalPath);

                    // These files are served with a content hash in the URL so can be cached for longer
                    bool isScriptOrStyle =
                        string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase);

                    if (isScriptOrStyle)
                    {
                        maxAge = TimeSpan.FromDays(365);
                    }
                }

                var headers = context.Context.Response.GetTypedHeaders();
                headers.CacheControl = new() { MaxAge = maxAge };
            };
        });

        builder.Services.AddAntiforgery((options) =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "_anti-forgery";
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.FormFieldName = "_anti-forgery";
            options.HeaderName = "x-anti-forgery";
        });

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRouting((options) =>
        {
            options.AppendTrailingSlash = true;
            options.LowercaseUrls = true;
        });

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHsts((options) =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = false;
                options.Preload = false;
            });
        }

        builder.Services.AddResponseCaching();

        builder.Services.AddTelemetry(builder.Environment);

        builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
        builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

        builder.Services.AddResponseCompression((options) =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Logging.AddTelemetry();

        builder.WebHost.CaptureStartupErrors(true);
        builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

        return builder;
    }

    /// <summary>
    /// Configures the specified <see cref="WebApplication"/> to use the website.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>
    /// The value passed by <paramref name="app"/> for chaining.
    /// </returns>
    public static WebApplication UseWebsite(this WebApplication app)
    {
        app.UseMiddleware<CustomHttpHeadersMiddleware>();

        bool isDevelopment = app.Environment.IsDevelopment();

        if (!isDevelopment)
        {
            app.UseExceptionHandler("/error");
        }

        app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();

            if (!string.Equals(app.Configuration["ForwardedHeaders_Enabled"], bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                app.UseHttpsRedirection();
            }
        }

        app.UseResponseCompression();

        app.UseRewriter(new RewriteOptions().AddRedirectToNonWww());

        app.UseStaticFiles();

        app.MapRedirects();

        app.MapGet("/version", static () =>
        {
            return new JsonObject()
            {
                ["applicationVersion"] = GitMetadata.Version,
                ["frameworkDescription"] = RuntimeInformation.FrameworkDescription,
                ["operatingSystem"] = new JsonObject()
                {
                    ["description"] = RuntimeInformation.OSDescription,
                    ["architecture"] = RuntimeInformation.OSArchitecture.ToString(),
                    ["version"] = Environment.OSVersion.VersionString,
                    ["is64Bit"] = Environment.Is64BitOperatingSystem,
                },
                ["process"] = new JsonObject()
                {
                    ["architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
                    ["is64BitProcess"] = Environment.Is64BitProcess,
                    ["isNativeAoT"] = !RuntimeFeature.IsDynamicCodeSupported,
                    ["isPrivilegedProcess"] = Environment.IsPrivilegedProcess,
                },
                ["dotnetVersions"] = new JsonObject()
                {
                    ["runtime"] = GetVersion<object>(),
                    ["aspNetCore"] = GetVersion<HttpContext>(),
                },
            };

            static string GetVersion<T>()
                => typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        });

        // HACK Workaround for https://github.com/dotnet/sdk/issues/40511
        app.MapGet(".well-known/{fileName}", (string fileName, IWebHostEnvironment environment) =>
        {
            var file = environment.WebRootFileProvider.GetFileInfo(Path.Combine("well-known", fileName));

            if (file.Exists && file.PhysicalPath is { Length: > 0 })
            {
                return Results.File(file.PhysicalPath, contentType: "application/json");
            }

            return Results.NotFound();
        });

        app.UseCookiePolicy(new()
        {
            HttpOnly = HttpOnlyPolicy.Always,
            Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
        });

        string[] methods = [HttpMethod.Get.Method, HttpMethod.Head.Method];

        app.MapMethods("/", methods, () => Results.Extensions.RazorSlice<Home>());
        app.MapMethods("/home/about", methods, () => Results.Extensions.RazorSlice<About>());
        app.MapMethods("/projects", methods, () => Results.Extensions.RazorSlice<Projects>());
        app.MapMethods("/tools", methods, () => Results.Extensions.RazorSlice<Tools>());

        app.MapMethods("/error", methods, (int? id) =>
        {
            int statusCode = StatusCodes.Status500InternalServerError;

            if (id is { } status &&
                status >= 400 && status < 599)
            {
                statusCode = status;
            }

            return Results.Extensions.RazorSlice<Error>(statusCode);
        });

        return app;
    }
}
