// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MartinCostello.Website.Integration;

/// <summary>
/// A class containing tests for loading resources in the website.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ResourceTests"/> class.
/// </remarks>
/// <param name="fixture">The fixture to use.</param>
/// <param name="outputHelper">The test output helper to use.</param>
[Collection(TestServerCollection.Name)]
public class ResourceTests(TestServerFixture fixture, ITestOutputHelper outputHelper) : IntegrationTest(fixture, outputHelper)
{
    [Theory]
    [InlineData("/", "text/html")]
    [InlineData("/apple-app-site-association", "application/json")]
    [InlineData("/assets/css/main.css", "text/css")]
    [InlineData("/assets/css/main.css.map", "text/plain")]
    [InlineData("/assets/js/main.js", "text/javascript")]
    [InlineData("/assets/js/main.js.map", "text/plain")]
    [InlineData("/.well-known/apple-app-site-association", "application/json")]
    [InlineData("/.well-known/assetlinks.json", "application/json")]
    [InlineData("BingSiteAuth.xml", "text/xml")]
    [InlineData("browserconfig.xml", "text/xml")]
    [InlineData("/bad-request.html", "text/html")]
    [InlineData("/error.html", "text/html")]
    [InlineData("/favicon.ico", "image/x-icon")]
    [InlineData("/googled1107923138d0b79.html", "text/html")]
    [InlineData("/home/about", "text/html")]
    [InlineData("/home/about/", "text/html")]
    [InlineData("/HOME/ABOUT", "text/html")]
    [InlineData("/humans.txt", "text/plain")]
    [InlineData("/keybase.txt", "text/plain")]
    [InlineData("/manifest.webmanifest", "application/manifest+json")]
    [InlineData("/not-found.html", "text/html")]
    [InlineData("/projects", "text/html")]
    [InlineData("/robots.txt", "text/plain")]
    [InlineData("/service-worker.js", "text/javascript")]
    [InlineData("/sitemap.xml", "text/xml")]
    [InlineData("/tools", "text/html")]
    [InlineData("/version", "application/json")]
    public async Task Can_Load_Resource_As_Get(string requestUri, string contentType)
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content.Headers.ContentType.ShouldNotBeNull();
        response.Content.Headers.ContentType.MediaType.ShouldNotBeNull();
        response.Content.Headers.ContentType.MediaType.ShouldBe(contentType);
    }

    [Theory]
    [InlineData("/", "text/html")]
    public async Task Can_Load_Resource_As_Head(string requestUri, string contentType)
    {
        // Arrange
        using var client = Fixture.CreateClient();
        using var message = new HttpRequestMessage(HttpMethod.Head, requestUri);

        // Act
        using var response = await client.SendAsync(message);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content.Headers.ContentType.ShouldNotBeNull();
        response.Content.Headers.ContentType.MediaType.ShouldNotBeNull();
        response.Content.Headers.ContentType.MediaType.ShouldBe(contentType);
    }

    [Theory]
    [InlineData("/Content/browserstack.svg", "https://cdn.martincostello.com/browserstack.svg")]
    public async Task Resource_Is_Redirect(string requestUri, string location)
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.OriginalString.ShouldNotBeNull();
        response.Headers.Location.OriginalString.ShouldStartWith(location);
    }

    [Fact]
    public async Task Response_Headers_Contains_Expected_Headers()
    {
        // Arrange
        string[] expectedHeaders =
        [
            "content-security-policy",
            "content-security-policy-report-only",
            "Permissions-Policy",
            "Referrer-Policy",
            "X-Content-Type-Options",
            "X-Download-Options",
            "X-Frame-Options",
            "X-Instance",
            "X-Request-Id",
            "X-Revision",
            "X-XSS-Protection",
        ];

        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync("/");

        // Assert
        foreach (string expected in expectedHeaders)
        {
            response.Headers.Contains(expected).ShouldBeTrue($"The '{expected}' response header was not found.");
        }
    }

    [Theory]
    [InlineData("/foo", HttpStatusCode.NotFound)]
    [InlineData("/error", HttpStatusCode.InternalServerError)]
    [InlineData("/error?id=399", HttpStatusCode.InternalServerError)]
    [InlineData("/error?id=400", HttpStatusCode.BadRequest)]
    [InlineData("/error?id=600", HttpStatusCode.InternalServerError)]
    public async Task Can_Load_Resource(string requestUri, HttpStatusCode expected)
    {
        // Arrange
        using var client = Fixture.CreateClient(new WebApplicationFactoryClientOptions() { AllowAutoRedirect = false });

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(expected, $"Incorrect status code for {requestUri}");
    }

    [Theory]
    [InlineData("/blog", "blog.martincostello.com")]
    [InlineData("/blog/foo", "blog.martincostello.com")]
    [InlineData("/gh", "github.com")]
    [InlineData("/github", "github.com")]
    [InlineData("/home/blog", "blog.martincostello.com")]
    [InlineData("/in", "www.linkedin.com")]
    [InlineData("/linkedin", "www.linkedin.com")]
    [InlineData("/linked-in", "www.linkedin.com")]
    [InlineData("/mvp", "mvp.microsoft.com")]
    [InlineData("/presentations", "github.com")]
    [InlineData("/talks", "github.com")]
    [InlineData("/tweet", "twitter.com")]
    [InlineData("/tweets", "twitter.com")]
    [InlineData("/twitter", "twitter.com")]
    [InlineData("/sessionize", "sessionize.com")]
    [InlineData("/slides", "github.com")]
    [InlineData("/so", "stackoverflow.com")]
    [InlineData("/stack", "stackoverflow.com")]
    [InlineData("/stackoverflow", "stackoverflow.com")]
    [InlineData("/stack-overflow", "stackoverflow.com")]
    [InlineData("/x", "twitter.com")]
    [InlineData("/youtube", "www.youtube.com")]
    public async Task Short_Link_Is_Redirected(string requestUri, string expectedHost)
    {
        // Arrange
        using var client = Fixture.CreateClient();

        // Act
        using var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.Host.ShouldBe(expectedHost);
    }

    [Theory]
    [InlineData("/.env")]
    [InlineData("/.git")]
    [InlineData("/.git/head")]
    [InlineData("/admin.php")]
    [InlineData("/admin")]
    [InlineData("/admin/")]
    [InlineData("/admin/index.php")]
    [InlineData("/administration")]
    [InlineData("/administration/")]
    [InlineData("/administration/index.php")]
    [InlineData("/administrator")]
    [InlineData("/administrator/")]
    [InlineData("/administrator/index.php")]
    [InlineData("/appsettings.json")]
    [InlineData("/bin/site.dll")]
    [InlineData("/obj/site.dll")]
    [InlineData("/package.json")]
    [InlineData("/package-lock.json")]
    [InlineData("/parameters.xml")]
    [InlineData("/web.config")]
    [InlineData("/wp-admin/blah")]
    [InlineData("/xmlrpc.php")]
    public async Task Crawler_Spam_Is_Redirected_To_YouTube(string requestUri)
    {
        // Arrange
        var methods = new[] { HttpMethod.Get, HttpMethod.Head, HttpMethod.Post };

        using var client = Fixture.CreateClient();

        foreach (var method in methods)
        {
            // Arrange
            using var message = new HttpRequestMessage(method, requestUri);

            if (method.Equals(HttpMethod.Post))
            {
                message.Content = new StringContent(string.Empty);
            }

            // Act
            using var response = await client.SendAsync(message);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            response.Headers.Location.ShouldNotBeNull();
            response.Headers.Location.Host.ShouldBe("www.youtube.com");
        }
    }
}
