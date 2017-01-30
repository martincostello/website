// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// A class representing the <see cref="Startup"/> class to use for tests.
    /// </summary>
    public class TestStartup : Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestStartup"/> class.
        /// </summary>
        /// <param name="env">The <see cref="IHostingEnvironment"/> to use.</param>
        public TestStartup(IHostingEnvironment env)
            : base(env)
        {
        }
    }
}
