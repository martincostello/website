// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace MartinCostello.Website.Integration.UI;

[Collection<HttpServerCollection>]
public sealed class AccessibilityTests(HttpServerFixture fixture, ITestOutputHelper outputHelper) : BrowserTest(fixture, outputHelper)
{
    private const string GeneratedGuid = "6c0b2e1a-37a2-4f0e-9a4d-3d0f6f0f7b11";
    private const string GeneratedHash = "LPJNul+wow4m6DsqxbninhsWHlwfp0JecwQzYpOLmCQ=";
    private const string GeneratedMachineKeyXml = "<machineKey validationKey=\"0123456789abcdef\" decryptionKey=\"fedcba9876543210\" validation=\"SHA1\" decryption=\"AES\" />";

    private static readonly string[] Tags =
    [
        "best-practice",
        "wcag2a",
        "wcag2aa",
        "wcag21a",
        "wcag21aa",
        "wcag22aa",
    ];

    [Theory]
    [InlineData("/")]
    [InlineData("/home/about")]
    [InlineData("/projects")]
    [InlineData("/tools")]
    [InlineData("/error")]
    public async Task Page_Has_No_Accessibility_Violations(string path)
    {
        await WithNavigatorAsync(
            "chromium",
            null,
            async (navigator) =>
            {
                // Arrange
                await ConfigureBrowserContextAsync(navigator.Page.Context);
                await navigator.NavigateToAsync(path);
                await WaitForPageAsync(navigator.Page);

                // Act and Assert
                await AssertNoViolationsAsync(navigator.Page, path);
            });
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/projects")]
    [InlineData("/tools")]
    public async Task Page_Has_No_Accessibility_Violations_On_Mobile_With_Navigation_Expanded(string path)
    {
        await WithNavigatorAsync(
            "chromium",
            null,
            async (navigator) =>
            {
                // Arrange
                var page = navigator.Page;

                await ConfigureBrowserContextAsync(page.Context);
                await page.SetViewportSizeAsync(375, 667);
                await navigator.NavigateToAsync(path);
                await WaitForPageAsync(page);

                var toggler = page.Locator(".navbar-toggler");
                await toggler.ClickAsync();
                await page.Locator("#site-navbar.show").WaitForAsync();

                // Act and Assert
                await AssertNoViolationsAsync(page, path + " (mobile)");
            });
    }

    [Fact]
    public async Task Navigation_Toggle_Reports_Its_State()
    {
        await WithNavigatorAsync(
            "chromium",
            null,
            async (navigator) =>
            {
                // Arrange
                var page = navigator.Page;

                await ConfigureBrowserContextAsync(page.Context);
                await page.SetViewportSizeAsync(375, 667);
                await navigator.NavigateToAsync("/");
                await WaitForPageAsync(page);

                var toggler = page.Locator(".navbar-toggler");

                // Act and Assert
                (await toggler.GetAttributeAsync("aria-expanded")).ShouldBe("false");

                await toggler.ClickAsync();
                await page.Locator("#site-navbar.show").WaitForAsync();

                (await toggler.GetAttributeAsync("aria-expanded")).ShouldBe("true");
            });
    }

    [Fact]
    public async Task Tools_Page_Has_No_Accessibility_Violations_With_Generated_Output()
    {
        await WithNavigatorAsync(
            "chromium",
            null,
            async (navigator) =>
            {
                // Arrange
                var page = navigator.Page;

                await ConfigureBrowserContextAsync(page.Context);
                await navigator.NavigateToAsync("/tools");
                await WaitForPageAsync(page);

                // Act - The output of the hash and machine key generators is hidden until it is generated,
                // and axe does not check hidden elements, so generate all of the output before scanning.
                await page.Locator("#generate-guid").ClickAsync();
                await page.Locator("#generate-hash").ClickAsync();
                await page.Locator("#generate-machine-key").ClickAsync();

                await page.Locator("#hash-container").WaitForAsync();
                await page.Locator("#machine-key-container").WaitForAsync();

                await page.WaitForFunctionAsync(
                    "() => document.getElementById('text-guid').value === '" + GeneratedGuid + "' && document.getElementById('text-hash').value === '" + GeneratedHash + "'");

                // Assert
                await AssertNoViolationsAsync(page, "/tools (with generated output)");
            });
    }

    /// <inheritdoc />
    protected override async Task ConfigureBrowserContextAsync(IBrowserContext context)
    {
        // The API is not what is being tested here, so return fixed responses for the tools
        await context.RouteAsync(
            "https://api.martincostello.com/**",
            (route) =>
            {
                string body = new Uri(route.Request.Url).AbsolutePath switch
                {
                    "/tools/guid" => $$"""{"guid":"{{GeneratedGuid}}"}""",
                    "/tools/hash" => $$"""{"hash":"{{GeneratedHash}}"}""",
                    "/tools/machinekey" => $$"""{"machineKeyXml":{{JsonSerializer.Serialize(GeneratedMachineKeyXml)}}}""",
                    _ => "{}",
                };

                return route.FulfillAsync(new() { ContentType = "application/json", Body = body });
            });

        // Avoid third-party widgets (which are outside the control of the site) affecting the results
        await context.RouteAsync(
            "https://buttons.github.io/**",
            (route) => route.FulfillAsync(new() { ContentType = "text/javascript", Body = string.Empty }));
    }

    private static async Task WaitForPageAsync(IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for the lazily-loaded images to be fully shown (including their fade-in
        // transition) so the final state of the page is scanned and not a transitional one
        await page.WaitForFunctionAsync(
            """
            () => Array.from(document.querySelectorAll('img.lazy')).every(
                (image) => !image.hasAttribute('data-original') && getComputedStyle(image).opacity === '1')
            """);
    }

    private static async Task AssertNoViolationsAsync(IPage page, string description)
    {
        var options = new AxeRunOptions()
        {
            RunOnly = new RunOnlyOptions()
            {
                Type = "tag",
                Values = [.. Tags],
            },
        };

        var results = await page.RunAxe(options);

        if (results.Violations.Length > 0)
        {
            var builder = new StringBuilder()
                .Append("Found ")
                .Append(results.Violations.Length)
                .Append(" accessibility violation(s) on ")
                .Append(description)
                .AppendLine(":");

            foreach (var violation in results.Violations)
            {
                builder
                    .AppendLine()
                    .Append(violation.Impact)
                    .Append(": ")
                    .Append(violation.Id)
                    .Append(" - ")
                    .AppendLine(violation.Help)
                    .Append("  ")
                    .AppendLine(violation.HelpUrl);

                foreach (var node in violation.Nodes)
                {
                    builder
                        .Append("  - ")
                        .AppendLine(node.Target.ToString());
                }
            }

            results.Violations.ShouldBeEmpty(builder.ToString());
        }
    }
}
