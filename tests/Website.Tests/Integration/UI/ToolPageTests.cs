// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using MartinCostello.Website.Pages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Playwright;

namespace MartinCostello.Website.Integration.UI;

/// <summary>
/// A class containing UI tests for tools page in the website.
/// </summary>
[Collection<HttpServerCollection>]
public sealed class ToolPageTests(HttpServerFixture fixture, ITestOutputHelper outputHelper) : BrowserTest(fixture, outputHelper)
{
    [Theory]
    [InlineData("Default", false)]
    [InlineData("Default", true)]
    [InlineData("Numeric", false)]
    [InlineData("With braces", false)]
    [InlineData("With parentheses", false)]
    [InlineData("Hexadecimal with braces", false)]
    public async Task Can_Generate_Guid(string format, bool useUpperCase)
    {
        // Arrange
        await AtPageAsync<ToolsPage>(
            async (page) =>
            {
                var generator = page.GuidGenerator();

                // Act
                string? actual = await generator.ValueAsync();

                // Assert
                Guid.TryParse(actual, out Guid firstGuid).ShouldBeTrue();
                firstGuid.ShouldNotBe(Guid.Empty);

                // Arrange
                await generator.WithFormatAsync(format);
                await generator.SetUpperCaseAsync(useUpperCase);

                // Act
                actual = await generator.GenerateAsync();

                // Assert
                Guid.TryParse(actual, out Guid secondGuid).ShouldBeTrue();
                secondGuid.ShouldNotBe(Guid.Empty);
                secondGuid.ShouldNotBe(firstGuid);

                if (useUpperCase)
                {
                    actual.ShouldBe(actual.ToUpperInvariant());
                }
                else
                {
                    actual.ShouldBe(actual.ToLowerInvariant());
                }
            });
    }

    [Theory]
    [InlineData("MD5", "Hexadecimal", "foo", "acbd18db4cc2f85cedef654fccc4a4d8")]
    [InlineData("SHA-1", "Base 64", "bar", "Ys23Ag/5IOWqZCw9QGaVDdHwH00=")]
    [InlineData("SHA-256", "Hexadecimal", "martincostello.com", "3b8143aa8119eaf0910aef5cade45dd0e6bb7b70e8d1c8c057bf3fc125248642")]
    public async Task Can_Generate_Hash(string algorithm, string format, string plaintext, string expected)
    {
        // Arrange
        await AtPageAsync<ToolsPage>(
            async (page) =>
            {
                var generator = await page.HashGenerator()
                    .WithAlgorithmAsync(algorithm)
                    .ThenAsync((p) => p.WithFormatAsync(format))
                    .ThenAsync((p) => p.WithPlaintextAsync(plaintext));

                // Act
                string actual = await generator.GenerateAsync();

                // Assert
                actual.ShouldBe(expected);
            });
    }

    [Theory]
    [InlineData("AES (256 bits)", "SHA-1", "AES", "SHA1")]
    [InlineData("3DES", "HMAC SHA-512", "3DES", "HMACSHA512")]
    public async Task Can_Generate_Machine_Key(
        string decryptionAlgorithm,
        string validationAlgorithm,
        string expectedDecryption,
        string expectedValidation)
    {
        // Arrange
        await AtPageAsync<ToolsPage>(
            async (page) =>
            {
                var generator = await page.MachineKeyGenerator()
                    .WithDecryptionAlgorithmAsync(decryptionAlgorithm)
                    .ThenAsync((p) => p.WithValidationAlgorithmAsync(validationAlgorithm));

                // Act
                string actual = await generator.GenerateAsync();

                // Assert
                actual.ShouldNotBeNullOrWhiteSpace();
                actual.ShouldStartWith("<machineKey ");
                actual.ShouldContain(" validationKey=\"");
                actual.ShouldContain(" decryptionKey=\"");
                actual.ShouldContain($"validation=\"{expectedValidation}\"");
                actual.ShouldContain($"decryption=\"{expectedDecryption}\"");
                actual.ShouldEndWith(" />");
            });
    }

    /// <inheritdoc />
    protected override async Task ConfigureBrowserContextAsync(IBrowserContext context)
    {
        await context.RouteAsync("https://api.martincostello.com/tools/guid**", HandleGuidRequestAsync);
        await context.RouteAsync("https://api.martincostello.com/tools/hash**", HandleHashRequestAsync);
        await context.RouteAsync("https://api.martincostello.com/tools/machinekey**", HandleMachineKeyRequestAsync);
    }

    private static async Task HandleGuidRequestAsync(IRoute route)
    {
        var uri = new Uri(route.Request.Url);
        var query = QueryHelpers.ParseQuery(uri.Query);

        string format = query.TryGetValue("format", out var value) ? value.ToString() : "D";
        bool uppercase = query.TryGetValue("uppercase", out value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

        var guid = Guid.NewGuid();
        string result = guid.ToString(format);

        if (uppercase)
        {
            result = result.ToUpperInvariant();
        }

        await route.FulfillAsync(new()
        {
            ContentType = "application/json",
            Body = $"{{\"guid\":{JsonSerializer.Serialize(result)}}}",
        });
    }

    private static async Task HandleHashRequestAsync(IRoute route)
    {
        string postData = route.Request.PostData ?? "{}";
        var body = JsonNode.Parse(postData);

        string algorithm = body?["algorithm"]?.GetValue<string>() ?? "SHA256";
        string format = body?["format"]?.GetValue<string>() ?? "base64";
        string plaintext = body?["plaintext"]?.GetValue<string>() ?? string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
#pragma warning disable CA5350, CA5351
        byte[] hash = algorithm switch
        {
            "MD5" => MD5.HashData(bytes),
            "SHA1" => SHA1.HashData(bytes),
            "SHA256" => SHA256.HashData(bytes),
            "SHA384" => SHA384.HashData(bytes),
            "SHA512" => SHA512.HashData(bytes),
            _ => SHA256.HashData(bytes),
        };
#pragma warning restore CA5350, CA5351

        string result = format switch
        {
            "hexadecimal" => Convert.ToHexStringLower(hash),
            _ => Convert.ToBase64String(hash),
        };

        await route.FulfillAsync(new()
        {
            ContentType = "application/json",
            Body = $"{{\"hash\":{JsonSerializer.Serialize(result)}}}",
        });
    }

    private static async Task HandleMachineKeyRequestAsync(IRoute route)
    {
        var uri = new Uri(route.Request.Url);
        var query = QueryHelpers.ParseQuery(uri.Query);

        string decryptionAlgorithm = query.TryGetValue("decryptionAlgorithm", out var d) ? d.ToString() : "AES-256";
        string validationAlgorithm = query.TryGetValue("validationAlgorithm", out var v) ? v.ToString() : "SHA1";

        int decryptionKeyBytes = decryptionAlgorithm switch
        {
            "3DES" => 24,
            "AES-128" => 16,
            "AES-192" => 24,
            "DES" => 8,
            _ => 32, // AES-256
        };

        int validationKeyBytes = validationAlgorithm switch
        {
            "HMACSHA384" => 48,
            "HMACSHA512" => 64,
            _ => 32,
        };

        string decryptionKey = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(decryptionKeyBytes));
        string validationKey = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(validationKeyBytes));

        string decryptionName = decryptionAlgorithm switch
        {
            "3DES" => "3DES",
            "DES" => "DES",
            _ => "AES",
        };

        string machineKeyXml = $"<machineKey validationKey=\"{validationKey}\" decryptionKey=\"{decryptionKey}\" validation=\"{validationAlgorithm}\" decryption=\"{decryptionName}\" />";

        await route.FulfillAsync(new()
        {
            ContentType = "application/json",
            Body = $"{{\"machineKeyXml\":{JsonSerializer.Serialize(machineKeyXml)}}}",
        });
    }
}
