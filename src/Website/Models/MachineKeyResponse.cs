// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the response from the <c>/tools/machinekey</c> API resource. This class cannot be inherited.
/// </summary>
public sealed class MachineKeyResponse
{
    /// <summary>
    /// Gets or sets a string containing the decryption key.
    /// </summary>
    [JsonPropertyName("decryptionKey")]
    [Required]
    public string DecryptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a string containing the validation key.
    /// </summary>
    [JsonPropertyName("validationKey")]
    [Required]
    public string ValidationKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a string containing the <c>&lt;machineKey&gt;</c> XML configuration element.
    /// </summary>
    [JsonPropertyName("machineKeyXml")]
    [Required]
    public string MachineKeyXml { get; set; } = string.Empty;
}
