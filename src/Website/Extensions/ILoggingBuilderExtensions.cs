﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// A class containing extension methods for the <see cref="ILoggingBuilder"/> interface. This class cannot be inherited.
/// </summary>
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// Adds OpenTelemetry logging to the specified <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
    /// <returns>
    /// The value of <paramref name="builder"/>.
    /// </returns>
    public static ILoggingBuilder AddTelemetry(this ILoggingBuilder builder)
    {
        return builder.AddOpenTelemetry((options) =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;

            options.SetResourceBuilder(ApplicationTelemetry.ResourceBuilder);
        });
    }
}
