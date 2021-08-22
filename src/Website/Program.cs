// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

#pragma warning disable SA1516

using System.IO.Compression;
using MartinCostello.Website.Extensions;
using MartinCostello.Website.Models;
using MartinCostello.Website.Options;
using MartinCostello.Website.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddOptions();

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<JsonOptions>((options) =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddAntiforgery((options) =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "_anti-forgery";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.FormFieldName = "_anti-forgery";
    options.HeaderName = "x-anti-forgery";
});

builder.Services.AddRazorPages();

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

builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

builder.Services.AddResponseCompression((options) =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddSingleton<IToolsService, ToolsService>();

builder.WebHost.ConfigureAppConfiguration(
    (context, builder) =>
    builder.AddApplicationInsightsSettings(developerMode: context.HostingEnvironment.IsDevelopment()));

builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

var app = builder.Build();

app.Lifetime.ApplicationStopped.Register(() => Serilog.Log.CloseAndFlush());
app.UseCustomHttpHeaders(app.Environment, app.Configuration, app.Services.GetRequiredService<IOptions<SiteOptions>>());

bool isDevelopment = app.Environment.IsDevelopment();

if (!isDevelopment)
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

app.UseHttpsRedirection()
   .UseHsts();

app.UseResponseCompression();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider,
    DefaultContentType = "application/json",
    OnPrepareResponse = (context) => SetCacheHeaders(context, isDevelopment),
    ServeUnknownFileTypes = true,
});

app.UseRouting();

app.MapRazorPages();

app.MapGet("/Content/browserstack.svg", (IOptions<SiteOptions> options) =>
{
    var builder = new UriBuilder(options.Value!.ExternalLinks!.Cdn!)
    {
        Scheme = Uri.UriSchemeHttps,
        Path = "browserstack.svg",
    };

    return Results.Redirect(builder.Uri.ToString());
});

app.MapGet("/home/blog", (IOptions<SiteOptions> options) =>
{
    return Results.Redirect(options.Value?.ExternalLinks?.Blog?.AbsoluteUri ?? "/");
});

app.MapGet("/tools/guid", (IToolsService service, string? format, bool? uppercase) =>
{
    return service.GenerateGuid(format, uppercase);
});

app.MapPost("/tools/hash", (HashRequest request, IToolsService service) =>
{
    return service.GenerateHash(request);
});

app.MapGet("/tools/machinekey", (IToolsService service, string? decryptionAlgorithm, string? validationAlgorithm) =>
{
    return service.GenerateMachineKey(decryptionAlgorithm, validationAlgorithm);
});

app.UseCookiePolicy(new()
{
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
});

app.MapGet("/tools/guid", (IToolsService service, string? format, bool? uppercase) =>
{
    return service.GenerateGuid(format, uppercase);
});

app.MapPost("/tools/hash", (HashRequest request, IToolsService service) =>
{
    return service.GenerateHash(request);
});

app.MapGet("/tools/machinekey", (IToolsService service, string? decryptionAlgorithm, string? validationAlgorithm) =>
{
    return service.GenerateMachineKey(decryptionAlgorithm, validationAlgorithm);
});

app.Run();

static void SetCacheHeaders(StaticFileResponseContext context, bool isDevelopment)
{
    var maxAge = TimeSpan.FromDays(7);

    if (context.File.Exists && !isDevelopment)
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
    headers.CacheControl = new CacheControlHeaderValue()
    {
        MaxAge = maxAge,
    };
}
