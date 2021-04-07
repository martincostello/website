// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Pages
{
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    public abstract class ToolComponent
    {
        protected ToolComponent(ApplicationNavigator navigator)
        {
            Navigator = navigator;
        }

        protected abstract By GeneratorSelector { get; }

        protected abstract By ResultSelector { get; }

        protected ApplicationNavigator Navigator { get; }

        public string Generate()
        {
            Navigator.Driver.FindElement(GeneratorSelector).Click();

            // Give the UI time to update
            var wait = new WebDriverWait(Navigator.Driver, TimeSpan.FromSeconds(3));
            wait.Until(driver => !string.IsNullOrWhiteSpace(GetResult(driver.FindElement(ResultSelector))));

            var element = Navigator.Driver.FindElement(ResultSelector);

            return GetResult(element);
        }

        protected abstract string GetResult(IWebElement element);
    }
}
