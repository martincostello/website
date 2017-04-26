// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Options;

    /// <summary>
    /// A class representing the controller for redirected resources.
    /// </summary>
    public class RedirectController : Controller
    {
        /// <summary>
        /// The <see cref="SiteOptions"/> to use. This field is read-only.
        /// </summary>
        private readonly SiteOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectController"/> class.
        /// </summary>
        /// <param name="options">The options to use.</param>
        public RedirectController(SiteOptions options)
        {
            _options = options;
        }

        [HttpGet]
        [Route("Content/browserstack.svg")]
        public IActionResult Browserstack() => Redirect(Url.CdnContent("browserstack.svg", _options));
    }
}
