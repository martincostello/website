// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using MartinCostello.Api.Models;
    using MartinCostello.Website.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A class representing the controller for the <c>/tools</c> page's API resources.
    /// </summary>
    [ApiController]
    [Route("tools")]
    public class ApiController : ControllerBase
    {
        /// <summary>
        /// The <see cref="IToolsService"/> to use. This field is read-only.
        /// </summary>
        private readonly IToolsService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiController"/> class.
        /// </summary>
        /// <param name="service">The service to use.</param>
        public ApiController(IToolsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Generates a GUID.
        /// </summary>
        /// <param name="format">The format for which to generate a GUID.</param>
        /// <param name="uppercase">Whether the output GUID should be uppercase.</param>
        /// <returns>
        /// An <see cref="ActionResult{GuidResponse}"/> containing the generated GUID.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(GuidResponse))]
        [ProducesResponseType(typeof(GuidResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("guid", Name = SiteRoutes.GenerateGuid)]
        public ActionResult<GuidResponse> Guid([FromQuery]string? format = null, [FromQuery]bool? uppercase = null)
            => _service.GenerateGuid(format, uppercase);

        /// <summary>
        /// Generates a hash of some plaintext for a specified hash algorithm and returns it in the required format.
        /// </summary>
        /// <param name="request">The hash request.</param>
        /// <returns>
        /// An <see cref="ActionResult{HashResponse}"/> containing the generated hash value.
        /// </returns>
        [Consumes("application/json", "text/json")]
        [HttpPost]
        [Produces("application/json", Type = typeof(HashResponse))]
        [ProducesResponseType(typeof(HashResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("hash", Name = SiteRoutes.GenerateHash)]
        public ActionResult<HashResponse> Hash([FromBody]HashRequest request)
            => _service.GenerateHash(request);

        /// <summary>
        /// Generates a machine key for a <c>Web.config</c> configuration file for ASP.NET.
        /// </summary>
        /// <param name="decryptionAlgorithm">The name of the decryption algorithm.</param>
        /// <param name="validationAlgorithm">The name of the validation algorithm.</param>
        /// <returns>
        /// An <see cref="ActionResult{MachineKeyResponse}"/> containing the generated machine key.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(MachineKeyResponse))]
        [ProducesResponseType(typeof(MachineKeyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Route("machinekey", Name = SiteRoutes.GenerateMachineKey)]
        public ActionResult<MachineKeyResponse> MachineKey([FromQuery]string? decryptionAlgorithm, [FromQuery]string? validationAlgorithm)
            => _service.GenerateMachineKey(decryptionAlgorithm, validationAlgorithm);
    }
}
