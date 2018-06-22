// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using System.Threading.Tasks;
    using Api.Models;
    using Microsoft.AspNetCore.Mvc;
    using Services;

    /// <summary>
    /// A class representing the controller for the <c>/tools</c> resource.
    /// </summary>
    public class ToolsController : Controller
    {
        /// <summary>
        /// The <see cref="IToolsService"/> to use. This field is read-only.
        /// </summary>
        private readonly IToolsService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolsController"/> class.
        /// </summary>
        /// <param name="service">The service to use.</param>
        public ToolsController(IToolsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets the result for the <c>/tools</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/tools</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.MetaDescription = ".NET Development Tools for generating GUIDs, machine keys and hashing text.";
            ViewBag.Title = ".NET Development Tools";

            return View();
        }

        /// <summary>
        /// Generates a GUID.
        /// </summary>
        /// <param name="format">The format for which to generate a GUID.</param>
        /// <param name="uppercase">Whether the output GUID should be uppercase.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated GUID.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(GuidResponse))]
        [Route("tools/guid", Name = SiteRoutes.GenerateGuid)]
        public IActionResult Guid([FromQuery]string format = null, [FromQuery]bool? uppercase = null)
        {
            return _service.GenerateGuid(format, uppercase);
        }

        /// <summary>
        /// Generates a hash of some plaintext for a specified hash algorithm and returns it in the required format.
        /// </summary>
        /// <param name="request">The hash request.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated hash value.
        /// </returns>
        [HttpPost]
        [Produces("application/json", Type = typeof(HashResponse))]
        [Route("tools/hash", Name = SiteRoutes.GenerateHash)]
        public Task<IActionResult> HashAsync([FromBody]HashRequest request)
        {
            return _service.GenerateHashAsync(request);
        }

        /// <summary>
        /// Generates a machine key for a <c>Web.config</c> configuration file for ASP.NET.
        /// </summary>
        /// <param name="decryptionAlgorithm">The name of the decryption algorithm.</param>
        /// <param name="validationAlgorithm">The name of the validation algorithm.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated machine key.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(MachineKeyResponse))]
        [Route("tools/machinekey", Name = SiteRoutes.GenerateMachineKey)]
        public IActionResult MachineKey([FromQuery]string decryptionAlgorithm, [FromQuery]string validationAlgorithm)
        {
            return _service.GenerateMachineKey(decryptionAlgorithm, validationAlgorithm);
        }
    }
}
