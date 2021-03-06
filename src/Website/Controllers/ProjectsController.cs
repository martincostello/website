// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.Website.Controllers
{
    /// <summary>
    /// A class representing the controller for the <c>/projects</c> resource.
    /// </summary>
    public class ProjectsController : Controller
    {
        /// <summary>
        /// Gets the result for the <c>/projects</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/projects</c> action.
        /// </returns>
        [HttpGet]
        [HttpHead]
        public IActionResult Index() => View();
    }
}
