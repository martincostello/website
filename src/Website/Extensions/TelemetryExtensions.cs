// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A class containing telemetry-related extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds telemetry services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure telemetry for.</param>
    /// <param name="environment">The current <see cref="IWebHostEnvironment"/>.</param>
    public static void AddTelemetry(this IServiceCollection services, IWebHostEnvironment environment)
    {
        var builder = services.AddOpenTelemetry();

        if (ApplicationTelemetry.IsOtlpCollectorConfigured())
        {
            builder.UseOtlpExporter();
        }

        builder.WithMetrics((builder) =>
               {
                   builder.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                          .AddAspNetCoreInstrumentation()
                          .AddHttpClientInstrumentation()
                          .AddProcessInstrumentation()
                          .AddMeter("System.Runtime")
                          .SetExemplarFilter(ExemplarFilterType.TraceBased);
               })
               .WithTracing((builder) =>
               {
                   builder.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                          .AddAspNetCoreInstrumentation()
                          .AddHttpClientInstrumentation()
                          .AddSource(ApplicationTelemetry.ServiceName);

                   if (environment.IsDevelopment())
                   {
                       builder.SetSampler(new AlwaysOnSampler());
                   }
               });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure<IServiceProvider>((options, _) => options.RecordException = true);
    }
}
