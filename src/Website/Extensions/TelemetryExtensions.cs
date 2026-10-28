// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website;
using Microsoft.Extensions.Diagnostics.Tracing;
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
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to configure telemetry for.</param>
    public static void AddTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMetrics();
        builder.Services.AddTracing((p) => p.AddConfiguration(builder.Configuration.GetSection("Tracing")));

        var telemetry = builder.Services.AddOpenTelemetry();

        if (ApplicationTelemetry.IsOtlpCollectorConfigured())
        {
            telemetry.UseOtlpExporter();
        }

        telemetry
            .WithMetrics((metrics) =>
            {
                metrics.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddProcessInstrumentation()
                       .SetExemplarFilter(ExemplarFilterType.TraceBased);
            })
            .WithTracing((tracing) =>
            {
                tracing.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddSource(ApplicationTelemetry.ServiceName);

                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }
            });

        builder.Services
            .AddOptions<HttpClientTraceInstrumentationOptions>()
            .Configure<IServiceProvider>((options, _) => options.RecordException = true);

        builder.Logging.AddTelemetry();
    }
}
