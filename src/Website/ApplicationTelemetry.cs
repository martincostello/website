// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;

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
}
