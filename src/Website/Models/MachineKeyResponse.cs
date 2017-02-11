// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Api.Models
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// A class representing the response from the <c>/tools/machinekey</c> API resource. This class cannot be inherited.
    /// </summary>
    public sealed class MachineKeyResponse
    {
        /// <summary>
        /// Gets or sets a string containing the decryption key.
        /// </summary>
        [JsonProperty("decryptionKey")]
        [Required]
        public string DecryptionKey { get; set; }

        /// <summary>
        /// Gets or sets a string containing the validation key.
        /// </summary>
        [JsonProperty("validationKey")]
        [Required]
        public string ValidationKey { get; set; }

        /// <summary>
        /// Gets or sets a string containing the <c>&lt;machineKey&gt;</c> XML configuration element.
        /// </summary>
        [JsonProperty("machineKeyXml")]
        [Required]
        public string MachineKeyXml { get; set; }
    }
}
