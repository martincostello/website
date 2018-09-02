// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Integration.UI.Pages
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public sealed class GuidComponent : ToolComponent
    {
        internal GuidComponent(ApplicationNavigator navigator)
            : base(navigator)
        {
        }

        protected override By GeneratorSelector => By.Id("generate-guid");

        protected override By ResultSelector => By.Id("text-guid");

        public GuidComponent WithFormat(string text)
        {
            var element = Navigator.Driver.FindElement(By.Id("guid-format"));
            new SelectElement(element).SelectByText(text);

            return this;
        }

        public GuidComponent ToggleCase()
        {
            Navigator.Driver.FindElement(By.CssSelector("[for='guid-uppercase']")).Click();
            return this;
        }

        public string Value() => GetResult(Navigator.Driver.FindElement(ResultSelector));

        protected override string GetResult(IWebElement element) => element.GetAttribute("value");
    }
}
