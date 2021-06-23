// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.Website.Pages
{
    public sealed class MachineKeyComponent : ToolComponent
    {
        internal MachineKeyComponent(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override string GeneratorSelector => "id=generate-machine-key";

        protected override string ResultSelector => "id=machine-key-xml";

        public async Task<MachineKeyComponent> WithDecryptionAlgorithmAsync(string text)
        {
            await Navigator.Page.SelectOptionAsync("id=key-decryption-algorithm", new SelectOptionValue() { Label = text });
            return this;
        }

        public async Task<MachineKeyComponent> WithValidationAlgorithmAsync(string text)
        {
            await Navigator.Page.SelectOptionAsync("id=key-validation-algorithm", new SelectOptionValue() { Label = text });
            return this;
        }

        public async Task<string?> ValueAsync()
            => await GetResultAsync(await Navigator.Page.WaitForSelectorAsync(ResultSelector));

        protected override async Task<string?> GetResultAsync(IElementHandle? element)
            => await element!.InnerTextAsync();
    }
}
