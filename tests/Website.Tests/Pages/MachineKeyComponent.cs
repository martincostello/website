// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Pages
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public sealed class MachineKeyComponent : ToolComponent
    {
        internal MachineKeyComponent(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override By GeneratorSelector => By.Id("generate-machine-key");

        protected override By ResultSelector => By.Id("machine-key-xml");

        public MachineKeyComponent WithDecryptionAlgorithm(string text)
        {
            var element = Navigator.Driver.FindElement(By.Id("key-decryption-algorithm"));
            new SelectElement(element).SelectByText(text);

            return this;
        }

        public MachineKeyComponent WithValidationAlgorithm(string text)
        {
            var element = Navigator.Driver.FindElement(By.Id("key-validation-algorithm"));
            new SelectElement(element).SelectByText(text);

            return this;
        }

        public string Value() => GetResult(Navigator.Driver.FindElement(ResultSelector));

        protected override string GetResult(IWebElement element) => element.Text;
    }
}
