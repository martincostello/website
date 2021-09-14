// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MartinCostello.Website.Integration;

/// <summary>
/// A class representing a factory for creating instances of the application.
/// </summary>
public class TestServerFixture : WebApplicationFactory<Services.IToolsService>, ITestOutputHelperAccessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestServerFixture"/> class.
    /// </summary>
    public TestServerFixture()
        : base()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    /// <inheritdoc />
    public ITestOutputHelper? OutputHelper { get; set; }

    /// <summary>
    /// Clears the current <see cref="ITestOutputHelper"/>.
    /// </summary>
    public virtual void ClearOutputHelper()
    {
        OutputHelper = null;
    }

    /// <summary>
    /// Sets the <see cref="ITestOutputHelper"/> to use.
    /// </summary>
    /// <param name="value">The <see cref="ITestOutputHelper"/> to use.</param>
    public virtual void SetOutputHelper(ITestOutputHelper value)
    {
        OutputHelper = value;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(ConfigureTests)
               .ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this))
               .UseSolutionRelativeContentRoot(Path.Combine("src", "Website"));
    }

    /// <summary>
    /// Configures the test settings.
    /// </summary>
    /// <param name="builder">The configuration builder to use.</param>
    private static void ConfigureTests(IConfigurationBuilder builder)
    {
        string directory = Path.GetDirectoryName(typeof(TestServerFixture).Assembly.Location) ?? ".";
        string fullPath = Path.Combine(directory, "testsettings.json");

        builder.AddJsonFile(fullPath);
    }
}
