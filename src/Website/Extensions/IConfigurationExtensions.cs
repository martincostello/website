// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Extensions;

/// <summary>
/// A class containing extension methods for <see cref=""/>. This class cannot be inherited.
/// </summary>
public static class IConfigurationExtensions
{
    /// <summary>
    /// Gets the configured Application Insights connection string.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/> to use.</param>
    /// <returns>
    /// The configured Application Insights connection string, if any.
    /// </returns>
    public static string ApplicationInsightsConnectionString(this IConfiguration config)
    {
        return config?["ApplicationInsights:ConnectionString"] ?? string.Empty;
    }

    /// <summary>
    /// Gets the name of the Azure datacenter the application is running in.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/> to use.</param>
    /// <returns>
    /// The name of the Azure datacenter the application is running in.
    /// </returns>
    public static string AzureDatacenter(this IConfiguration config)
    {
        return config?["Azure:Datacenter"] ?? "local";
    }

    /// <summary>
    /// Gets the name of the Azure environment the application is running in.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/> to use.</param>
    /// <returns>
    /// The name of the Azure environment the application is running in.
    /// </returns>
    public static string AzureEnvironment(this IConfiguration config)
    {
        return config?["Azure:Environment"] ?? "local";
    }
}
