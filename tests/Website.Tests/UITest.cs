// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using MartinCostello.Website.Pages;

namespace MartinCostello.Website;

/// <summary>
/// The base class for browser tests.
/// </summary>
public abstract class UITest : IAsyncLifetime, IDisposable
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
    /// Gets the base address of the website under test.
    /// </summary>
    protected abstract Uri ServerAddress { get; }

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

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        InstallPlaywright();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> as an asynchronous operation.
    /// </summary>
    /// <param name="browserType">The type of the browser to run the test with.</param>
    /// <param name="test">The delegate to the test that will use the navigator.</param>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation to run the test.
    /// </returns>
    protected async Task WithNavigatorAsync(
        string browserType,
        Func<ApplicationNavigator, Task> test,
        [CallerMemberName] string? testName = null)
    {
        var fixture = new BrowserFixture(Output);

        await fixture.WithPageAsync(
            browserType,
            async (page) =>
            {
                var navigator = new ApplicationNavigator(ServerAddress, page);
                await test(navigator);
            },
            testName);
    }

    /// <summary>
    /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
    /// </summary>
    /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
    /// <param name="test">The delegate to the test that will use the navigator.</param>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation to run the test.
    /// </returns>
    protected async Task AtPageAsync<T>(
        Func<ApplicationNavigator, T, Task> test,
        [CallerMemberName] string? testName = null)
        where T : PageBase
    {
        await WithNavigatorAsync(
            "chromium",
            async (navigator) =>
            {
                T? page = Activator.CreateInstance(typeof(T), navigator) as T;
                await page!.NavigateAsync();

                await test(navigator, page!);
            },
            testName: testName);
    }

    /// <summary>
    /// Runs the specified test with a new instance of <see cref="ApplicationNavigator"/> for the specified page type.
    /// </summary>
    /// <typeparam name="T">The type of the page to navigate to for the test.</typeparam>
    /// <param name="test">The delegate to the test that will use the navigator.</param>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation to run the test.
    /// </returns>
    protected async Task AtPageAsync<T>(
        Func<T, Task> test,
        [CallerMemberName] string? testName = null)
        where T : PageBase
    {
        await AtPageAsync<T>(
            async (_, page) => await test(page),
            testName: testName);
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

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
