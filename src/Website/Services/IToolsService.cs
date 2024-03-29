﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Models;

namespace MartinCostello.Website.Services;

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
    /// An <see cref="IResult"/> containing the generated GUID or an error.
    /// </returns>
    IResult GenerateGuid(string? format, bool? uppercase);

    /// <summary>
    /// Generates a hash of some plaintext for a specified hash algorithm
    /// and returns it in the required format.
    /// </summary>
    /// <param name="request">The hash request.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing the generated hash value or an error.
    /// </returns>
    IResult GenerateHash(HashRequest request);

    /// <summary>
    /// Generates a machine key for a <c>Web.config</c> configuration file for ASP.NET.
    /// </summary>
    /// <param name="decryptionAlgorithm">The name of the decryption algorithm.</param>
    /// <param name="validationAlgorithm">The name of the validation algorithm.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing the generated machine key or an error.
    /// </returns>
    IResult GenerateMachineKey(string? decryptionAlgorithm, string? validationAlgorithm);
}
