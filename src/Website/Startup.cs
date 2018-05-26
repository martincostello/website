// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A class representing the startup logic for the application.
    /// </summary>
    public class Startup : StartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/> to use.</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
            : base(configuration, hostingEnvironment)
        {
        }
    }
}
