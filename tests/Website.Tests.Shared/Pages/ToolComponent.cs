// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.Website.Pages;

public abstract class ToolComponent(ApplicationNavigator navigator)
{
    protected abstract string GeneratorSelector { get; }

    protected abstract string ResultSelector { get; }

    protected ApplicationNavigator Navigator { get; } = navigator;

    public async Task<string> GenerateAsync()
    {
        string? oldValue = await GetResultAsync(await Navigator.Page.QuerySelectorAsync(ResultSelector));

        await Navigator.Page.ClickAsync(GeneratorSelector);

        // Give the UI time to update
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (!cts.IsCancellationRequested)
        {
            string? currentValue = await GetResultAsync(await Navigator.Page.QuerySelectorAsync(ResultSelector));

            if (!string.IsNullOrWhiteSpace(currentValue) && !string.Equals(currentValue, oldValue, StringComparison.Ordinal))
            {
                return currentValue;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), cts.Token);
        }

        return null!;
    }

    protected abstract Task<string?> GetResultAsync(IElementHandle? element);
}
