// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using MartinCostello.Website;
using MartinCostello.Website.Middleware;
using MartinCostello.Website.Models;
using MartinCostello.Website.Options;
using MartinCostello.Website.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTelemetry(builder.Environment);

builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

builder.Services.AddResponseCompression((options) =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddSingleton<IToolsService, ToolsService>();

builder.Logging.AddTelemetry();

builder.WebHost.CaptureStartupErrors(true);
builder.WebHost.ConfigureKestrel((p) => p.AddServerHeader = false);

var app = builder.Build();

app.UseMiddleware<CustomHttpHeadersMiddleware>();

bool isDevelopment = app.Environment.IsDevelopment();

if (!isDevelopment)
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

app.UseHttpsRedirection()
   .UseHsts();

app.UseResponseCompression();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.MapRedirects();

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

app.Run();
