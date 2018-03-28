// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Integration
{
    using System;
    using Microsoft.AspNetCore.Mvc.Testing;

    /// <summary>
    /// A test fixture representing an HTTP server hosting the website.
    /// </summary>
    public class HttpServerFixture : WebApplicationTestFixture<TestStartup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        public HttpServerFixture()
            : base("src/Website")
        {
            Client.BaseAddress = new Uri("https://localhost");
        }
    }
}
