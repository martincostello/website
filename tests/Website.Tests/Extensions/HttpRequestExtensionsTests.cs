// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Options;
using Microsoft.AspNetCore.Http;

namespace MartinCostello.Website.Extensions;

[Category("Unit")]
public static class HttpRequestExtensionsTests
{
    [Theory]
    [InlineData("martincostello.com", "/", null, "https://martincostello.com/")]
    [InlineData("martincostello.com", "", null, "https://martincostello.com/")]
    [InlineData("martincostello.com", "/home/about", null, "https://martincostello.com/home/about/")]
    [InlineData("martincostello.com", "/home/about/", null, "https://martincostello.com/home/about/")]
    [InlineData("MartinCostello.com", "/Home/About", null, "https://martincostello.com/home/about/")]
    [InlineData("martincostello.com:443", "/tools", null, "https://martincostello.com/tools/")]
    [InlineData("localhost:5001", "/tools", null, "https://localhost:5001/tools/")]
    [InlineData("[::1]:5001", "/tools", null, "https://[::1]:5001/tools/")]
    [InlineData("martincostello.com", "/home/about", "", "https://martincostello.com/")]
    [InlineData("martincostello.com", "/home/about", "projects", "https://martincostello.com/projects/")]
    [InlineData("martincostello.com", "/home/about", "/Projects/", "https://martincostello.com/projects/")]
    public static void Canonical_Returns_Correct_Value(string host, string path, string? canonicalPath, string expected)
    {
        // Arrange
        var request = CreateRequest(host, path);

        // Act
        string actual = request.Canonical(canonicalPath);

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "", false, "")]
    [InlineData(null, null, false, "")]
    [InlineData(null, "/assets/js/main.js", false, "/assets/js/main.js")]
    [InlineData(null, "~/projects", false, "/projects")]
    [InlineData("/app", "~/projects", false, "/app/projects")]
    [InlineData("/app/", "~/projects", false, "/app/projects")]
    [InlineData("/app", "/projects", false, "/projects")]
    public static void Content_Returns_Correct_Value_Without_Version(string? pathBase, string? contentPath, bool appendVersion, string expected)
    {
        // Arrange
        var request = CreateRequest("martincostello.com", "/", pathBase);

        // Act
        string? actual = request.Content(contentPath, appendVersion);

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "", "?v=")]
    [InlineData(null, null, "?v=")]
    [InlineData(null, "/assets/js/main.js", "/assets/js/main.js?v=")]
    [InlineData(null, "~/manifest.webmanifest", "/manifest.webmanifest?v=")]
    [InlineData("/app", "~/manifest.webmanifest", "/app/manifest.webmanifest?v=")]
    [InlineData("/app/", "~/manifest.webmanifest", "/app/manifest.webmanifest?v=")]
    public static void Content_Returns_Correct_Value_With_Version(string? pathBase, string? contentPath, string expectedPrefix)
    {
        // Arrange
        var request = CreateRequest("martincostello.com", "/", pathBase);

        // Act
        string? actual = request.Content(contentPath, appendVersion: true);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldStartWith(expectedPrefix);
        actual.Length.ShouldBeGreaterThan(expectedPrefix.Length);
        actual.ShouldEndWith(VersionQuery(request));
    }

    [Theory]
    [InlineData(null, "", "https://cdn.martincostello.com/?v=")]
    [InlineData(null, "favicon.ico", "https://cdn.martincostello.com/favicon.ico?v=")]
    [InlineData(null, "/favicon.ico", "https://cdn.martincostello.com/favicon.ico?v=")]
    [InlineData(null, "~/favicon.ico", "https://cdn.martincostello.com/favicon.ico?v=")]
    [InlineData("/app", "~/favicon.ico", "https://cdn.martincostello.com/app/favicon.ico?v=")]
    [InlineData("/app/", "~/favicon.ico", "https://cdn.martincostello.com/app/favicon.ico?v=")]
    [InlineData("/app", "/favicon.ico", "https://cdn.martincostello.com/favicon.ico?v=")]
    public static void CdnContent_Returns_Correct_Value(string? pathBase, string contentPath, string expectedPrefix)
    {
        // Arrange
        var request = CreateRequest("martincostello.com", "/", pathBase);
        var options = CreateOptions("https://cdn.martincostello.com/");

        // Act
        string actual = request.CdnContent(contentPath, options);

        // Assert
        actual.ShouldStartWith(expectedPrefix);
        actual.ShouldEndWith(VersionQuery(request));
    }

    [Fact]
    public static void CdnContent_Includes_Non_Default_Port()
    {
        // Arrange
        var request = CreateRequest("martincostello.com", "/");
        var options = CreateOptions("https://localhost:8443/");

        // Act
        string actual = request.CdnContent("~/favicon.ico", options);

        // Assert
        actual.ShouldStartWith("https://localhost:8443/favicon.ico?v=");
    }

    [Fact]
    public static void CdnContent_Returns_Empty_String_If_No_Cdn_Configured()
    {
        // Arrange
        var request = CreateRequest("martincostello.com", "/");
        var options = new SiteOptions();

        // Act
        string actual = request.CdnContent("~/favicon.ico", options);

        // Assert
        actual.ShouldBe(string.Empty);
    }

    private static string VersionQuery(HttpRequest request)
    {
        string? version = request.Content(string.Empty, appendVersion: true);

        version.ShouldNotBeNull();
        version.ShouldStartWith("?v=");
        version.Length.ShouldBeGreaterThan("?v=".Length);

        return version;
    }

    private static HttpRequest CreateRequest(string host, string path, string? pathBase = null)
    {
        var context = new DefaultHttpContext();

        context.Request.Scheme = "https";
        context.Request.Host = new HostString(host);
        context.Request.Path = path;
        context.Request.PathBase = pathBase ?? string.Empty;

        return context.Request;
    }

    private static SiteOptions CreateOptions(string cdn)
    {
        return new()
        {
            ExternalLinks = new()
            {
                Cdn = new(cdn, UriKind.Absolute),
            },
        };
    }
}
