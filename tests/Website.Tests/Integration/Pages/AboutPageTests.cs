// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Integration.Pages
{
    /// <summary>
    /// A class containing tests for the about page.
    /// </summary>
    public class AboutPageTests : HtmlPageTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPageTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        public AboutPageTests(HttpServerFixture fixture)
            : base(fixture)
        {
        }

        /// <inheritdoc/>
        protected override string Path => "/home/about/";
    }
}
