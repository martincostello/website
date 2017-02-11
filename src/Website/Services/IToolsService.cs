﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Services
{
    using System.Threading.Tasks;
    using Api.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the service for the tools page.
    /// </summary>
    public interface IToolsService
    {
        /// <summary>
        /// Generates a GUID.
        /// </summary>
        /// <param name="format">The format for which to generate a GUID.</param>
        /// <param name="uppercase">Whether the output GUID should be uppercase.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated GUID.
        /// </returns>
        IActionResult GenerateGuid(string format, bool? uppercase);

        /// <summary>
        /// Generates a hash of some plaintext for a specified hash algorithm and returns
        /// it in the required format as an asynchronous operation.
        /// </summary>
        /// <param name="request">The hash request.</param>
        /// <returns>
        /// A <see cref="Task"/> that returns an <see cref="IActionResult"/> containing the generated hash value.
        /// </returns>
        Task<IActionResult> GenerateHashAsync(HashRequest request);

        /// <summary>
        /// Generates a machine key for a <c>Web.config</c> configuration file for ASP.NET.
        /// </summary>
        /// <param name="decryptionAlgorithm">The name of the decryption algorithm.</param>
        /// <param name="validationAlgorithm">The name of the validation algorithm.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated machine key.
        /// </returns>
        IActionResult GenerateMachineKey(string decryptionAlgorithm, string validationAlgorithm);

        /// <summary>
        /// Returns the URI of the API endpoint.
        /// </summary>
        /// <returns>
        /// A string containing the URL of the API endpoint.
        /// </returns>
        string GetApiUri();
    }
}
