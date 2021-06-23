// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MartinCostello.Website.Controllers
{
    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Gets the result for the <c>/home/about</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/home/about</c> action.
        /// </returns>
        [HttpGet]
        [HttpHead]
        public IActionResult About() => View();

        /// <summary>
        /// Gets the result for the <c>/home/blog</c> action.
        /// </summary>
        /// <param name="options">The site options to use.</param>
        /// <returns>
        /// The result for the <c>/home/blog</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Blog([FromServices] IOptions<SiteOptions> options)
        {
            return Redirect(options.Value?.ExternalLinks?.Blog?.AbsoluteUri ?? "/");
        }

        /// <summary>
        /// Gets the result for the <c>/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/</c> action.
        /// </returns>
        [HttpGet]
        [HttpHead]
        public IActionResult Index() => View();
    }
}
