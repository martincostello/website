// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.Website.Pages;

public sealed class HashComponent : ToolComponent
{
    internal HashComponent(ApplicationNavigator navigator)
        : base(navigator)
    {
    }

    protected override string GeneratorSelector => "id=generate-hash";

    protected override string ResultSelector => "id=text-hash";

    public async Task<HashComponent> WithAlgorithmAsync(string text)
    {
        await Navigator.Page.SelectOptionAsync("id=hash-algorithm", new SelectOptionValue() { Label = text });
        return this;
    }

    public async Task<HashComponent> WithFormatAsync(string text)
    {
        await Navigator.Page.SelectOptionAsync("id=hash-format", new SelectOptionValue() { Label = text });
        return this;
    }

    public async Task<HashComponent> WithPlaintextAsync(string text)
    {
        await Navigator.Page.FillAsync("id=hash-plaintext", text);
        return this;
    }

    public async Task<string?> ValueAsync()
        => await GetResultAsync(await Navigator.Page.WaitForSelectorAsync(ResultSelector));

    protected override async Task<string?> GetResultAsync(IElementHandle? element)
        => await element!.GetAttributeAsync("value");
}
