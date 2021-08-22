// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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
using BadRequestObjectResult = Microsoft.AspNetCore.Mvc.BadRequestObjectResult;

namespace MartinCostello.Website
{
    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
        /// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/> to use.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Gets the current configuration.
        /// </summary>
        private IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the current hosting environment.
        /// </summary>
        private IWebHostEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="applicationLifetime">The <see cref="IHostApplicationLifetime"/> to use.</param>
        /// <param name="options">The snapshot of <see cref="SiteOptions"/> to use.</param>
        public void Configure(
            IApplicationBuilder app,
            IHostApplicationLifetime applicationLifetime,
            IOptions<SiteOptions> options)
        {
            applicationLifetime.ApplicationStopped.Register(() => Serilog.Log.CloseAndFlush());
            app.UseCustomHttpHeaders(HostingEnvironment, Configuration, options);

            if (HostingEnvironment.IsDevelopment())
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

            app.UseResponseCompression();

            app.UseStaticFiles(CreateStaticFileOptions());

            app.UseRouting();

            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapRazorPages();

                endpoints.MapGet("/Content/browserstack.svg", (context) =>
                {
                    var options = context.RequestServices.GetRequiredService<IOptions<SiteOptions>>();

                    var builder = new UriBuilder(options.Value!.ExternalLinks!.Cdn!)
                    {
                        Scheme = Uri.UriSchemeHttps,
                        Path = "browserstack.svg",
                    };

                    context.Response.Redirect(builder.Uri.ToString());
                    return Task.CompletedTask;
                });

                endpoints.MapGet("/home/blog", (context) =>
                {
                    var options = context.RequestServices.GetRequiredService<IOptions<SiteOptions>>();
                    context.Response.Redirect(options.Value?.ExternalLinks?.Blog?.AbsoluteUri ?? "/");
                    return Task.CompletedTask;
                });

                endpoints.MapGet("/tools/guid", async (context) =>
                {
                    string? format = context.Request.Query["format"];
                    bool? uppercase = null;

                    if (context.Request.Query.TryGetValue("uppercase", out var valueAsString))
                    {
                        if (bool.TryParse(valueAsString, out bool valueAsBoolean))
                        {
                            uppercase = valueAsBoolean;
                        }
                    }

                    var service = context.RequestServices.GetRequiredService<IToolsService>();
                    var response = service.GenerateGuid(format, uppercase);

                    // TODO Decouple from MVC in #713
                    if (response.Result is BadRequestObjectResult badRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(badRequest.Value);
                        return;
                    }

                    await context.Response.WriteAsJsonAsync(response.Value);
                });

                endpoints.MapPost("/tools/hash", async (context) =>
                {
                    var request = await context.Request.ReadFromJsonAsync<HashRequest>();

                    var service = context.RequestServices.GetRequiredService<IToolsService>();
                    var response = service.GenerateHash(request ?? new HashRequest());

                    // TODO Decouple from MVC in #713
                    if (response.Result is BadRequestObjectResult badRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(badRequest.Value);
                        return;
                    }

                    await context.Response.WriteAsJsonAsync(response.Value);
                });

                endpoints.MapGet("/tools/machinekey", async (context) =>
                {
                    string? decryptionAlgorithm = context.Request.Query["decryptionAlgorithm"];
                    string? validationAlgorithm = context.Request.Query["validationAlgorithm"];

                    var service = context.RequestServices.GetRequiredService<IToolsService>();
                    var response = service.GenerateMachineKey(decryptionAlgorithm, validationAlgorithm);

                    // TODO Decouple from MVC in #713
                    if (response.Result is BadRequestObjectResult badRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(badRequest.Value);
                        return;
                    }

                    await context.Response.WriteAsJsonAsync(response.Value);
                });
            });

            app.UseCookiePolicy(CreateCookiePolicy());
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddOptions();

            services.Configure<SiteOptions>(Configuration.GetSection("Site"));
            services.Configure<JsonOptions>((options) =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = false;
                options.SerializerOptions.WriteIndented = true;
            });

            services.AddAntiforgery((options) =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "_anti-forgery";
                options.Cookie.SecurePolicy = CookiePolicy();
                options.FormFieldName = "_anti-forgery";
                options.HeaderName = "x-anti-forgery";
            });

            services.AddRazorPages();

            services.AddRouting((options) =>
            {
                options.AppendTrailingSlash = true;
                options.LowercaseUrls = true;
            });

            if (!HostingEnvironment.IsDevelopment())
            {
                services.AddHsts((options) =>
                {
                    options.MaxAge = TimeSpan.FromDays(365);
                    options.IncludeSubDomains = false;
                    options.Preload = false;
                });
            }

            services.AddResponseCaching();

            services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
            services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

            services.AddResponseCompression((options) =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddSingleton<IToolsService, ToolsService>();
        }

        /// <summary>
        /// Sets the cache headers for static files.
        /// </summary>
        /// <param name="context">The static file response context to set the headers for.</param>
        private void SetCacheHeaders(StaticFileResponseContext context)
        {
            var maxAge = TimeSpan.FromDays(7);

            if (context.File.Exists && HostingEnvironment.IsProduction())
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

        /// <summary>
        /// Configures the options for serving static content.
        /// </summary>
        /// <returns>
        /// The <see cref="StaticFileOptions"/> to use.
        /// </returns>
        private StaticFileOptions CreateStaticFileOptions()
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

        /// <summary>
        /// Creates the <see cref="CookiePolicyOptions"/> to use.
        /// </summary>
        /// <returns>
        /// The <see cref="CookiePolicyOptions"/> to use for the application.
        /// </returns>
        private CookiePolicyOptions CreateCookiePolicy()
        {
            return new CookiePolicyOptions()
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookiePolicy(),
            };
        }

        /// <summary>
        /// Creates the <see cref="CookieSecurePolicy"/> to use.
        /// </summary>
        /// <returns>
        /// The <see cref="CookieSecurePolicy"/> to use for the application.
        /// </returns>
        private CookieSecurePolicy CookiePolicy()
            => HostingEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    }
}
