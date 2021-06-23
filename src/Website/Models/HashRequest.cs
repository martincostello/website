// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.Website.Models
{
    /// <summary>
    /// A class representing a request for the <c>/tools/hash</c> API resource. This class cannot be inherited.
    /// </summary>
    public sealed class HashRequest
    {
        /// <summary>
        /// Gets or sets the name of the hash algorithm to use.
        /// </summary>
        [JsonPropertyName("algorithm")]
        public string Algorithm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the format in which to return the hash.
        /// </summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plaintext value to generate the hash from.
        /// </summary>
        [JsonPropertyName("plaintext")]
        public string Plaintext { get; set; } = string.Empty;
    }
}
