// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using Pages;
    using Xunit.Abstractions;

    /// <summary>
    /// The base class for browser tests.
    /// </summary>
    public abstract class UITest : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UITest"/> class.
        /// </summary>
        /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
        protected UITest(ITestOutputHelper outputHelper)
        {
            Output = outputHelper;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UITest"/> class.
        /// </summary>
        ~UITest()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="ITestOutputHelper"/> to use.
        /// </summary>
        protected ITestOutputHelper Output { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ApplicationNavigator"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="ApplicationNavigator"/> to use for tests.
        /// </returns>
        protected abstract ApplicationNavigator CreateNavigator();

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/>.
        /// </summary>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        protected void WithNavigator(Action<ApplicationNavigator> test, [CallerMemberName] string? testName = null)
        {
            using ApplicationNavigator navigator = CreateNavigator();

            try
            {
                test(navigator);
            }
            catch (Exception)
            {
                TakeScreenshot(navigator.Driver, testName);
                OutputLogs(navigator.Driver);
                throw;
            }
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
        /// </summary>
        /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        protected void AtPage<T>(Action<ApplicationNavigator, T> test, [CallerMemberName] string? testName = null)
            where T : PageBase
        {
            WithNavigator(
                (navigator) =>
                {
                    var page = Activator.CreateInstance(typeof(T), navigator) as T;
                    page!.Navigate();

                    test(navigator, page!);
                },
                testName: testName);
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
        /// </summary>
        /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        protected void AtPage<T>(Action<T> test, [CallerMemberName] string? testName = null)
            where T : PageBase
        {
            AtPage<T>((_, page) => test(page), testName: testName);
        }

        /// <summary>
        /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> as an asynchronous operation.
        /// </summary>
        /// <param name="test">The delegate to the test that will use the navigator.</param>
        /// <param name="testName">The name of the test method.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to run the test.
        /// </returns>
        protected async Task WithNavigatorAsync(Func<ApplicationNavigator, Task> test, [CallerMemberName] string? testName = null)
        {
            using ApplicationNavigator navigator = CreateNavigator();

            try
            {
                await test(navigator);
            }
            catch (Exception)
            {
                TakeScreenshot(navigator.Driver, testName);
                OutputLogs(navigator.Driver);
                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // No-op
        }

        private void OutputLogs(IWebDriver driver)
        {
            try
            {
                var logs = driver.Manage().Logs;

                var allEntries = new List<Tuple<string, LogEntry>>();

                foreach (string logKind in logs.AvailableLogTypes)
                {
                    var logEntries = logs.GetLog(logKind)
                        .Select((p) => Tuple.Create(logKind, p))
                        .ToList();

                    allEntries.AddRange(logEntries);
                }

                foreach (var logEntry in allEntries.OrderBy((p) => p.Item2.Timestamp))
                {
                    var logKind = logEntry.Item1;
                    var entry = logEntry.Item2;
                    Output.WriteLine($"[{entry.Timestamp:u}] {logKind} - {entry.Level}: {entry.Message}");
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Output.WriteLine($"Failed to output driver logs: {ex}");
            }
        }

        private void TakeScreenshot(IWebDriver driver, string? testName)
        {
            try
            {
                if (driver is ITakesScreenshot camera)
                {
                    Screenshot screenshot = camera.GetScreenshot();

                    string? directory = Path.GetDirectoryName(typeof(UITest).Assembly.Location) ?? ".";
                    string fileName = $"{testName}_{DateTimeOffset.UtcNow:YYYY-MM-dd-HH-mm-ss}.png";

                    fileName = Path.Combine(directory, fileName);

                    screenshot.SaveAsFile(fileName);
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Output.WriteLine($"Failed to take screenshot: {ex}");
            }
        }
    }
}
