// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Integration
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// The base class for integration tests for HTML pages.
    /// </summary>
    public abstract class HtmlPageTest : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlPageTest"/> class.
        /// </summary>
        protected HtmlPageTest()
            : base()
        {
        }

        /// <summary>
        /// Gets the path to the page.
        /// </summary>
        protected abstract string Path { get; }

        [Fact]
        public async Task Can_Load_Page_As_Html()
        {
            using (var response = await GetPageAsync())
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.Equal("utf-8", response.Content.Headers.ContentType.CharSet);
                Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
            }
        }

        [Fact]
        public async Task Response_Headers_Contains_Expected_Headers()
        {
            string[] excpectedHeaders = new[]
            {
                "content-security-policy",
                "X-Content-Type-Options",
                "X-Datacenter",
                "X-Download-Options",
                "X-Frame-Options",
                "X-Instance",
                "X-Request-Duration",
                "X-Request-Id",
                "X-Revision",
                "X-XSS-Protection",
            };

            using (var response = await GetPageAsync())
            {
                foreach (string expected in excpectedHeaders)
                {
                    Assert.True(response.Headers.Contains(expected), $"The '{expected}' response header was not found");
                }
            }
        }

        /// <summary>
        /// Loads the page as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the operation to load the page.
        /// </returns>
        protected async Task<HttpResponseMessage> GetPageAsync() => await Client.GetAsync(Path);
    }
}
