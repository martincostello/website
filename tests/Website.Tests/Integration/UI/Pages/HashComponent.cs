// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Integration.UI.Pages
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public sealed class HashComponent : ToolComponent
    {
        internal HashComponent(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override By GeneratorSelector => By.Id("generate-hash");

        protected override By ResultSelector => By.Id("text-hash");

        public HashComponent WithAlgorithm(string text)
        {
            var element = Navigator.Driver.FindElement(By.Id("hash-algorithm"));
            new SelectElement(element).SelectByText(text);

            return this;
        }

        public HashComponent WithFormat(string text)
        {
            var element = Navigator.Driver.FindElement(By.Id("hash-format"));
            new SelectElement(element).SelectByText(text);

            return this;
        }

        public HashComponent WithPlaintext(string text)
        {
            Navigator.Driver.FindElement(By.Id("hash-plaintext")).SendKeys(text);
            return this;
        }

        public string Value() => GetResult(Navigator.Driver.FindElement(ResultSelector));

        protected override string GetResult(IWebElement element) => element.GetAttribute("value");
    }
}
