// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.EndToEnd
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using MartinCostello.Website.Pages;
    using Xunit;

    public class WebsiteFixture
    {
        private const string WebsiteUrl = "WEBSITE_URL";

        public WebsiteFixture()
        {
            string url = Environment.GetEnvironmentVariable(WebsiteUrl) ?? string.Empty;

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? address))
            {
                ServerAddress = address;
            }
        }

        public Uri? ServerAddress { get; }

        public HttpClient CreateClient()
        {
            Skip.If(ServerAddress is null, $"The {WebsiteUrl} environment variable is not set or is not a valid absolute URI.");

            var client = new HttpClient()
            {
                BaseAddress = ServerAddress,
            };

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "MartinCostello.Website.Tests",
                    "1.0.0+" + GitMetadata.Commit));

            return client;
        }

        public ApplicationNavigator CreateNavigator()
        {
            Skip.If(ServerAddress is null, $"The {WebsiteUrl} environment variable is not set or is not a valid absolute URI.");

            return new ApplicationNavigator(ServerAddress, WebDriverFactory.CreateWebDriver());
        }
    }
}
