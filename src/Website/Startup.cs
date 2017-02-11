﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.CookiePolicy;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using NodaTime;
    using Options;
    using Services;

    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The name of the default CORS policy.
        /// </summary>
        internal const string DefaultCorsPolicyName = "DefaultCorsPolicy";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            bool isDevelopment = env.IsDevelopment();

            if (isDevelopment)
            {
                builder.AddUserSecrets("martincostello.com");
            }

            builder.AddApplicationInsightsSettings(developerMode: isDevelopment);

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        /// <summary>
        /// Gets or sets the current configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the current hosting environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to use.</param>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> to use.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="options">The snapshot of <see cref="SiteOptions"/> to use.</param>
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment environment,
            ILoggerFactory loggerFactory,
            IOptionsSnapshot<SiteOptions> options)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            app.UseCustomHttpHeaders(environment, Configuration, options);

            if (environment.IsDevelopment())
            {
                loggerFactory.AddDebug();

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error")
                   .UseStatusCodePagesWithReExecute("/error", "?id={0}");
            }

            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    DefaultContentType = "application.json",
                    ServeUnknownFileTypes = true,
                    OnPrepareResponse = (context) =>
                        {
                            var headers = context.Context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue()
                            {
                                MaxAge = TimeSpan.FromDays(7)
                            };
                        }
                });

            app.UseForwardedHeaders(
                new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

            app.UseHttpMethodOverride();

            app.UseMvc(
                (routes) =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });

            app.UseCookiePolicy(CreateCookiePolicy());
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to use.</param>
        /// <returns>
        /// The <see cref="IServiceProvider"/> to use.
        /// </returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddOptions();
            services.Configure<SiteOptions>(Configuration.GetSection("Site"));

            services.AddAntiforgery(
                (p) =>
                {
                    p.CookieName = "_anti-forgery";
                    p.FormFieldName = "_anti-forgery";
                    p.HeaderName = "x-anti-forgery";
                    p.RequireSsl = !HostingEnvironment.IsDevelopment();
                });

            services
                .AddMemoryCache()
                .AddDistributedMemoryCache()
                .AddCors(ConfigureCors)
                .AddMvc(ConfigureMvc)
                .AddJsonOptions((p) => services.AddSingleton(ConfigureJsonFormatter(p)));

            services.AddRouting(
                (p) =>
                {
                    p.AppendTrailingSlash = true;
                    p.LowercaseUrls = true;
                });

            services
                .AddResponseCaching()
                .AddResponseCompression();

            services.AddSingleton<IConfiguration>((_) => Configuration);
            services.AddSingleton<IClock>((_) => SystemClock.Instance);
            services.AddSingleton((p) => new BowerVersions(p.GetRequiredService<IHostingEnvironment>()));

            services.AddScoped((p) => p.GetRequiredService<IHttpContextAccessor>().HttpContext);
            services.AddScoped((p) => p.GetRequiredService<IOptionsSnapshot<SiteOptions>>().Value);
            services.AddScoped<IToolsService, ToolsService>();

            var builder = new ContainerBuilder();

            builder.Populate(services);

            var container = builder.Build();
            ServiceProvider = container.Resolve<IServiceProvider>();

            return ServiceProvider;
        }

        /// <summary>
        /// Configures the JSON serializer for MVC.
        /// </summary>
        /// <param name="options">The <see cref="MvcJsonOptions"/> to configure.</param>
        /// <returns>
        /// The <see cref="JsonSerializerSettings"/> to use.
        /// </returns>
        private static JsonSerializerSettings ConfigureJsonFormatter(MvcJsonOptions options)
        {
            // Make JSON easier to read for debugging at the expense of larger payloads
            options.SerializerSettings.Formatting = Formatting.Indented;

            // Omit nulls to reduce payload size
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Explicitly define behavior when serializing DateTime values
            options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";   // Only return DateTimes to a 1 second precision

            return options.SerializerSettings;
        }

        /// <summary>
        /// Configures CORS.
        /// </summary>
        /// <param name="corsOptions">The <see cref="CorsOptions"/> to configure.</param>
        private void ConfigureCors(CorsOptions corsOptions)
        {
            var siteOptions = ServiceProvider.GetService<SiteOptions>();

            corsOptions.AddPolicy(
                DefaultCorsPolicyName,
                (builder) =>
                {
                    builder
                        .WithExposedHeaders(siteOptions?.Api?.Cors?.ExposedHeaders ?? Array.Empty<string>())
                        .WithHeaders(siteOptions?.Api?.Cors?.Headers ?? Array.Empty<string>())
                        .WithMethods(siteOptions?.Api?.Cors?.Methods ?? Array.Empty<string>());

                    if (HostingEnvironment.IsDevelopment())
                    {
                        builder.AllowAnyOrigin();
                    }
                    else
                    {
                        builder.WithOrigins(siteOptions?.Api?.Cors?.Origins ?? Array.Empty<string>());
                    }
                });
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
                Secure = HostingEnvironment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
            };
        }
    }
}
