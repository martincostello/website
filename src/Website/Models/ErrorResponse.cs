// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the error response from an API resource. This class cannot be inherited.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    [JsonPropertyName("statusCode")]
    [Required]
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request Id.
    /// </summary>
    [JsonPropertyName("requestId")]
    [Required]
    public string RequestId { get; set; } = string.Empty;
}
