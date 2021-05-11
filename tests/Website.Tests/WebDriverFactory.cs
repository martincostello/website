// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using System;
    using System.IO;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    /// <summary>
    /// A factory class for creating instances of <see cref="IWebDriver"/>. This class cannot be inherited.
    /// </summary>
    internal static class WebDriverFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="IWebDriver"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="IWebDriver"/> to use for tests.
        /// </returns>
        public static IWebDriver CreateWebDriver()
        {
            string chromeDriverDirectory = Path.GetDirectoryName(typeof(WebDriverFactory).Assembly.Location) ?? ".";

            var options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true,
            };

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                options.AddArgument("--headless");
            }

            options.SetLoggingPreference(LogType.Browser, LogLevel.All);

#if DEBUG
            options.SetLoggingPreference(LogType.Client, LogLevel.All);
            options.SetLoggingPreference(LogType.Driver, LogLevel.All);
            options.SetLoggingPreference(LogType.Profiler, LogLevel.All);
            options.SetLoggingPreference(LogType.Server, LogLevel.All);
#endif

            var driver = new ChromeDriver(chromeDriverDirectory, options);

            try
            {
                var timeout = TimeSpan.FromSeconds(10);
                var timeouts = driver.Manage().Timeouts();

                timeouts.AsynchronousJavaScript = timeout;
                timeouts.ImplicitWait = timeout;
                timeouts.PageLoad = timeout;
            }
            catch (Exception)
            {
                driver.Dispose();
                throw;
            }

            return driver;
        }
    }
}
