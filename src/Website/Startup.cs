// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using System;
    using System.IO;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.CookiePolicy;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Options;
    using Services;

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
            applicationLifetime.ApplicationStopped.Register(OnApplicationStopped);
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

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                });

            app.UseHttpMethodOverride();

            app.UseStaticFiles(CreateStaticFileOptions());

            app.UseRouting();

            app.UseEndpoints(
                (endpoints) =>
                {
                    endpoints.MapDefaultControllerRoute();
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

            services.AddAntiforgery(
                (p) =>
                {
                    p.Cookie.HttpOnly = true;
                    p.Cookie.Name = "_anti-forgery";
                    p.Cookie.SecurePolicy = CookiePolicy();
                    p.FormFieldName = "_anti-forgery";
                    p.HeaderName = "x-anti-forgery";
                });

            services.AddControllersWithViews(ConfigureMvc)
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                    .AddJsonOptions(ConfigureJsonFormatter);

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            if (!HostingEnvironment.IsDevelopment())
            {
                services.AddHsts(
                    (p) =>
                    {
                        p.MaxAge = TimeSpan.FromDays(365);
                        p.IncludeSubDomains = false;
                        p.Preload = false;
                    });
            }

            services.AddResponseCaching()
                    .AddResponseCompression();

            services.AddSingleton<IToolsService, ToolsService>();
            services.AddHttpContextAccessor();
            services.AddScoped((p) => p.GetRequiredService<IOptions<SiteOptions>>().Value);
        }

        /// <summary>
        /// Configures the JSON serializer for MVC.
        /// </summary>
        /// <param name="options">The <see cref="JsonOptions"/> to configure.</param>
        private static void ConfigureJsonFormatter(JsonOptions options)
        {
            // Make JSON easier to read for debugging at the expense of larger payloads
            options.JsonSerializerOptions.WriteIndented = true;

            // Omit nulls to reduce payload size
            options.JsonSerializerOptions.IgnoreNullValues = true;

            // Opt-out of case insensitivity on property names
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
        }

        /// <summary>
        /// Configures MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcOptions"/> to configure.</param>
        private void ConfigureMvc(MvcOptions options)
        {
            if (!HostingEnvironment.IsDevelopment())
            {
                options.Filters.Add(new RequireHttpsAttribute());
            }
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

        /// <summary>
        /// Handles the application being stopped.
        /// </summary>
        private void OnApplicationStopped()
        {
            Serilog.Log.CloseAndFlush();
        }
    }
}
