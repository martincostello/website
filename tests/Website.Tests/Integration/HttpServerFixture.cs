// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace MartinCostello.Website.Integration;

/// <summary>
/// A test fixture representing an HTTP server hosting the application. This class cannot be inherited.
/// </summary>
public sealed class HttpServerFixture : TestServerFixture
{
    public HttpServerFixture()
    {
        UseKestrel(
            (server) => server.Listen(
                IPAddress.Loopback, 0, (listener) => listener.UseHttps(
                    (https) => https.ServerCertificate = LoadDevelopmentCertificate())));
    }

    /// <summary>
    /// Gets the server address of the application.
    /// </summary>
    public Uri ServerAddress
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress;
        }
    }

    private static X509Certificate2 LoadDevelopmentCertificate()
    {
        var metadata = typeof(HttpServerFixture).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToArray();

        var fileName = metadata.First((p) => p.Key is "DevCertificateFileName").Value!;
        var password = metadata.First((p) => p.Key is "DevCertificatePassword").Value;

        return X509CertificateLoader.LoadPkcs12(File.ReadAllBytes(fileName), password);
    }
}
