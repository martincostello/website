// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using OpenTelemetry.Resources;

namespace MartinCostello.Website;

/// <summary>
/// A class containing telemetry information for the application.
/// </summary>
public static class ApplicationTelemetry
{
    /// <summary>
    /// The name of the service.
    /// </summary>
    public static readonly string ServiceName = "Website";

    /// <summary>
    /// The version of the service.
    /// </summary>
    public static readonly string ServiceVersion = GitMetadata.Version.Split('+')[0];

    /// <summary>
    /// The custom activity source for the service.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// Gets the <see cref="ResourceBuilder"/> to use for telemetry.
    /// </summary>
    public static ResourceBuilder ResourceBuilder { get; } = ResourceBuilder.CreateDefault()
        .AddService(ServiceName, ServiceName, ServiceVersion)
        .AddAttributes([new KeyValuePair<string, object>("deployment.environment.name", Environment.GetEnvironmentVariable("Azure__Environment") ?? string.Empty)])
        .AddAzureAppServiceDetector()
        .AddContainerDetector()
        .AddHostDetector()
        .AddOperatingSystemDetector()
        .AddProcessRuntimeDetector();

    /// <summary>
    /// Returns whether an OTLP collector is configured.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if an OTLP collector is configured; otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsOtlpCollectorConfigured()
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));
}
