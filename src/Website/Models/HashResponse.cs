// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the response from the <c>/tools/hash</c> API resource. This class cannot be inherited.
/// </summary>
public sealed class HashResponse
{
    /// <summary>
    /// Gets or sets a string containing the generated hash value in the requested format.
    /// </summary>
    [JsonPropertyName("hash")]
    [Required]
    public string Hash { get; set; } = string.Empty;
}
