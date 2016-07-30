// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Options;

    /// <summary>
    /// A class representing the controller for the <c>/</c> resource.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// The URL of the blog. This field is read-only.
        /// </summary>
        private readonly Uri _blogUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="options">The site options to use.</param>
        public HomeController(SiteOptions options)
        {
            _blogUri = options?.ExternalLinks?.Blog;
        }

        /// <summary>
        /// Gets the result for the <c>/home/about</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/home/about</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult About() => View();

        /// <summary>
        /// Gets the result for the <c>/home/blog</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/home/blog</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Blog() => Redirect(_blogUri?.AbsoluteUri ?? "/");

        /// <summary>
        /// Gets the result for the <c>/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();
    }
}
