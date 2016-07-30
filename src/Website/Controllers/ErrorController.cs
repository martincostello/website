// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/error</c> resource.
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Gets the result for the <c>/error</c> action.
        /// </summary>
        /// <param name="id">The optional HTTP status code associated with the error.</param>
        /// <returns>
        /// The result for the <c>/error</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index(int? id) => View("Error", id ?? 500);
    }
}
