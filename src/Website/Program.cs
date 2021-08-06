// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO;
using MartinCostello.Website.Extensions;
using MartinCostello.Website.Options;
using MartinCostello.Website.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddOptions();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));

builder.Services.AddAntiforgery(
    (p) =>
    {
        p.Cookie.HttpOnly = true;
        p.Cookie.Name = "_anti-forgery";
        p.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        p.FormFieldName = "_anti-forgery";
        p.HeaderName = "x-anti-forgery";
    });

builder.Services.AddControllersWithViews((options) =>
                {
                    if (!builder.Environment.IsDevelopment())
                    {
                        options.Filters.Add(new RequireHttpsAttribute());
                    }
                })
                .AddJsonOptions((options) =>
                {
                    // Make JSON easier to read for debugging at the expense of larger payloads
                    options.JsonSerializerOptions.WriteIndented = true;

                    // Omit nulls to reduce payload size
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                    // Opt-out of case insensitivity on property names
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                });

builder.Services.AddRouting(
    (p) =>
    {
        p.AppendTrailingSlash = true;
        p.LowercaseUrls = true;
    });

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(
        (p) =>
        {
            p.MaxAge = TimeSpan.FromDays(365);
            p.IncludeSubDomains = false;
            p.Preload = false;
        });
}

builder.Services.AddResponseCaching()
                .AddResponseCompression();

builder.Services.AddSingleton<IToolsService, ToolsService>();

builder.WebHost
    .CaptureStartupErrors(true)
    .ConfigureAppConfiguration((context, builder) => builder.AddApplicationInsightsSettings(developerMode: context.HostingEnvironment.IsDevelopment()))
    .ConfigureKestrel((p) => p.AddServerHeader = false);

var app = builder.Build();

var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
applicationLifetime.ApplicationStopped.Register(() => Serilog.Log.CloseAndFlush());

var options = app.Services.GetRequiredService<IOptions<SiteOptions>>();
app.UseCustomHttpHeaders(app.Environment, app.Configuration, options);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error")
       .UseStatusCodePagesWithReExecute("/error", "?id={0}");

    app.UseHsts()
       .UseHttpsRedirection();
}

app.UseForwardedHeaders(
    new ForwardedHeadersOptions()
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    });

app.UseHttpMethodOverride();

app.UseResponseCompression();

app.UseStaticFiles(CreateStaticFileOptions());

app.UseRouting();

app.UseEndpoints((endpoints) => endpoints.MapDefaultControllerRoute());

app.UseCookiePolicy(CreateCookiePolicy(app.Environment.IsDevelopment()));

app.Run();

void SetCacheHeaders(StaticFileResponseContext context)
{
    var maxAge = TimeSpan.FromDays(7);
    var env = context.Context.RequestServices.GetRequiredService<IHostEnvironment>();

    if (context.File.Exists && env.IsProduction())
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

StaticFileOptions CreateStaticFileOptions()
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".webmanifest"] = "application/manifest+json";

    return new StaticFileOptions()
    {
        ContentTypeProvider = provider,
        DefaultContentType = "application/json",
        OnPrepareResponse = SetCacheHeaders,
        ServeUnknownFileTypes = true,
    };
}

CookiePolicyOptions CreateCookiePolicy(bool isDevelopment)
{
    return new CookiePolicyOptions()
    {
        HttpOnly = HttpOnlyPolicy.Always,
        Secure = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
    };
}
