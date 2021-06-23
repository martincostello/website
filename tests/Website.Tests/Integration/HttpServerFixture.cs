// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MartinCostello.Website.Integration
{
    /// <summary>
    /// A test fixture representing an HTTP server hosting the application. This class cannot be inherited.
    /// </summary>
    public sealed class HttpServerFixture : TestServerFixture
    {
        private IHost? _host;
        private bool _disposed;

        /// <summary>
        /// Gets the server address of the application.
        /// </summary>
        public Uri ServerAddress
        {
            get
            {
                EnsureServer();
                return ClientOptions.BaseAddress;
            }
        }

        /// <inheritdoc />
        public override IServiceProvider? Services
        {
            get
            {
                EnsureServer();
                return _host!.Services!;
            }
        }

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureKestrel(
                (p) => p.ConfigureHttpsDefaults(
                    (r) => r.ServerCertificate = new X509Certificate2("localhost-dev.pfx", "Pa55w0rd!")));

            // Configure the server address for the server to
            // listen on for HTTPS requests on a dynamic port.
            builder.UseUrls("https://127.0.0.1:0");
        }

        /// <inheritdoc />
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureWebHost((p) => p.UseKestrel());

            _host = builder.Build();
            _host.Start();

            var server = _host.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            ClientOptions.BaseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();

            // The base class still needs a separate host using TestServer
            var testHost = CreateHostBuilder()
                .ConfigureWebHost((p) => p.UseTestServer())
                .Build();

            testHost.Start();

            return testHost;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    _host?.Dispose();
                }

                _disposed = true;
            }
        }

        private void EnsureServer()
        {
            if (_host is null)
            {
                using (CreateDefaultClient())
                {
                }
            }
        }
    }
}
