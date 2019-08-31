// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A class representing the response from the <c>/tools/guid</c> API resource. This class cannot be inherited.
    /// </summary>
    public sealed class GuidResponse
    {
        /// <summary>
        /// Gets or sets the generated GUID value.
        /// </summary>
        [JsonPropertyName("guid")]
        [Required]
        public string Guid { get; set; } = string.Empty;
    }
}
