// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MartinCostello.Website
{
    /// <summary>
    /// A class representing the entry-point to the application. This class cannot be inherited.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry-point to the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <returns>
        /// The exit code from the application.
        /// </returns>
        public static int Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Console.Error.WriteLine($"Unhandled exception: {ex}");
                return -1;
            }
        }

        /// <summary>
        /// Creates the <see cref="IHostBuilder"/> for the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <returns>
        /// The <see cref="IHostBuilder"/> to use for the application.
        /// </returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) => builder.ConfigureLogging(context))
                .ConfigureWebHostDefaults(
                    (webBuilder) =>
                    {
                        webBuilder.CaptureStartupErrors(true)
                                  .ConfigureAppConfiguration((context, builder) => builder.AddApplicationInsightsSettings(developerMode: context.HostingEnvironment.IsDevelopment()))
                                  .ConfigureKestrel((p) => p.AddServerHeader = false)
                                  .UseStartup<Startup>();
                    });
        }
    }
}
