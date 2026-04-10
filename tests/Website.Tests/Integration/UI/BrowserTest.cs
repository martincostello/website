// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Playwright;

namespace MartinCostello.Website.Integration.UI;

/// <summary>
/// The base class for browser integration tests.
/// </summary>
[Collection<HttpServerCollection>]
public abstract class BrowserTest : UITest
{
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserTest"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use.</param>
    /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
    protected BrowserTest(HttpServerFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Fixture = fixture;
        Fixture.SetOutputHelper(outputHelper);
    }

    /// <summary>
    /// Gets the <see cref="HttpServerFixture"/> to use.
    /// </summary>
    protected HttpServerFixture Fixture { get; }

    /// <inheritdoc />
    protected override Uri ServerAddress => Fixture.ServerAddress;

    /// <inheritdoc />
    protected override async Task ConfigureBrowserContextAsync(IBrowserContext context)
    {
        await context.RouteAsync("https://api.martincostello.com/tools/guid**", HandleGuidRequestAsync);
        await context.RouteAsync("https://api.martincostello.com/tools/hash**", HandleHashRequestAsync);
        await context.RouteAsync("https://api.martincostello.com/tools/machinekey**", HandleMachineKeyRequestAsync);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Fixture?.ClearOutputHelper();
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }

    private static async Task HandleGuidRequestAsync(IRoute route)
    {
        var uri = new Uri(route.Request.Url);
        var query = QueryHelpers.ParseQuery(uri.Query);

        string format = query.TryGetValue("format", out var f) ? f.ToString() : "D";
        bool uppercase = query.TryGetValue("uppercase", out var u) && string.Equals(u, "true", StringComparison.OrdinalIgnoreCase);

        var guid = Guid.NewGuid();
        string result = format switch
        {
            "N" => guid.ToString("N"),
            "B" => guid.ToString("B"),
            "P" => guid.ToString("P"),
            "X" => guid.ToString("X"),
            _ => guid.ToString("D"),
        };

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
            "hexadecimal" => Convert.ToHexString(hash).ToLowerInvariant(),
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

        string decryptionKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(decryptionKeyBytes)).ToLowerInvariant();
        string validationKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(validationKeyBytes)).ToLowerInvariant();

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
