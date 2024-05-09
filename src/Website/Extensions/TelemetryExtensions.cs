// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Azure.Monitor.OpenTelemetry.Exporter;
using MartinCostello.Website;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.ResourceDetectors.Azure;
using OpenTelemetry.Resources;
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
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ApplicationTelemetry.ServiceName, serviceVersion: ApplicationTelemetry.ServiceVersion)
            .AddDetector(new AppServiceResourceDetector());

        if (IsAzureMonitorConfigured())
        {
            services.Configure<AzureMonitorExporterOptions>(
                (p) => p.ConnectionString = AzureMonitorConnectionString());
        }

        var telemetry = services
            .AddOpenTelemetry()
            .WithMetrics((builder) =>
            {
                builder.SetResourceBuilder(resourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation();

                if (IsAzureMonitorConfigured())
                {
                    builder.AddAzureMonitorMetricExporter();
                }

                if (IsOtlpCollectorConfigured())
                {
                    builder.AddOtlpExporter();
                }
            })
            .WithTracing((builder) =>
            {
                builder.SetResourceBuilder(resourceBuilder)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddSource(ApplicationTelemetry.ServiceName);

                if (environment.IsDevelopment())
                {
                    builder.SetSampler(new AlwaysOnSampler());
                }

                if (IsAzureMonitorConfigured())
                {
                    builder.AddAzureMonitorTraceExporter();
                }

                if (IsOtlpCollectorConfigured())
                {
                    builder.AddOtlpExporter();
                }
            });

        services.AddOptions<HttpClientTraceInstrumentationOptions>()
                .Configure<IServiceProvider>((options, _) => options.RecordException = true);
    }

    /// <summary>
    /// Returns whether Azure Monitor is configured.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if Azure Monitor is configured; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsAzureMonitorConfigured()
        => !string.IsNullOrEmpty(AzureMonitorConnectionString());

    /// <summary>
    /// Returns whether an OTLP collector is configured.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if OLTP is configured; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsOtlpCollectorConfigured()
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));

    private static string? AzureMonitorConnectionString()
        => Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
}
