﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Pages
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public sealed class GuidComponent : ToolComponent
    {
        internal GuidComponent(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string GeneratorSelector => "id=generate-guid";

        protected override string ResultSelector => "id=text-guid";

        public async Task<GuidComponent> WithFormatAsync(string text)
        {
            await Navigator.Page.SelectOptionAsync("id=guid-format", new SelectOptionValue() { Label = text });
            return this;
        }

        public async Task<GuidComponent> ToggleCaseAsync()
        {
            await Navigator.Page.ClickAsync("[for='guid-uppercase']");
            return this;
        }

        public async Task<string?> ValueAsync() =>
            await GetResultAsync(await Navigator.Page.WaitForSelectorAsync(ResultSelector));

        protected override async Task<string?> GetResultAsync(IElementHandle? element)
            => await element!.GetAttributeAsync("value");
    }
}
