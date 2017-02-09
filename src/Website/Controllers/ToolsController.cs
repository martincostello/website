// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using Api.Models;
    using Microsoft.AspNetCore.Mvc;
    using Options;

    /// <summary>
    /// A class representing the controller for the <c>/tools</c> resource.
    /// </summary>
    public class ToolsController : Controller
    {
        /// <summary>
        /// An <see cref="IDictionary{K, V}"/> containing the sizes of the decryption and validation hashes for machine keys.
        /// </summary>
        private static readonly IDictionary<string, int> HashSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "3DES-D", 24 },
            { "3DES-V", 24 },
            { "AES-128-D", 16 },
            { "AES-192-D", 24 },
            { "AES-256-D", 32 },
            { "AES-V", 32 },
            { "DES-D", 32 },
            { "MD5-V", 16 },
            { "HMACSHA256-V", 32 },
            { "HMACSHA384-V", 48 },
            { "HMACSHA512-V", 64 },
            { "SHA1-V", 64 },
        };

        /// <summary>
        /// The URL of the API. This field is read-only.
        /// </summary>
        private readonly Uri _apiUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolsController"/> class.
        /// </summary>
        /// <param name="options">The site options to use.</param>
        public ToolsController(SiteOptions options)
        {
            _apiUri = options?.ExternalLinks?.Api;
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
            ViewBag.Title = ".NET Development Tools";
            ViewBag.MetaDescription = ".NET Development Tools for generating GUIDs, machine keys and hashing text.";
            ViewBag.ToolsUri = (_apiUri?.IsAbsoluteUri == true ? _apiUri.AbsoluteUri : "/") + "tools";

            return View();
        }

        /// <summary>
        /// Generates a GUID.
        /// </summary>
        /// <param name="format">The format for which to generate a GUID.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the generated GUID.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(GuidResponse))]
        [Route("tools/guid")]
        public IActionResult Guid([FromQuery]string format = null, [FromQuery]bool? uppercase = null)
        {
            string guid;

            try
            {
                guid = System.Guid.NewGuid().ToString(format ?? "D");
            }
            catch (FormatException)
            {
                return BadRequest($"The specified format '{format}' is invalid.");
            }

            if (uppercase == true)
            {
                guid = guid.ToUpperInvariant();
            }

            var value = new GuidResponse()
            {
                Guid = guid,
            };

            return new OkObjectResult(value);
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
        [Route("tools/hash")]
        public IActionResult Hash([FromBody]HashRequest request)
        {
            if (request == null)
            {
                return BadRequest("No hash request specified.");
            }

            if (string.IsNullOrWhiteSpace(request.Algorithm))
            {
                return BadRequest("No hash algorithm name specified.");
            }

            if (string.IsNullOrWhiteSpace(request.Format))
            {
                return BadRequest("No hash output format specified.");
            }

            bool formatAsBase64;

            switch (request.Format.ToUpperInvariant())
            {
                case "BASE64":
                    formatAsBase64 = true;
                    break;

                case "HEXADECIMAL":
                    formatAsBase64 = false;
                    break;

                default:
                    return BadRequest($"The specified hash format '{request.Format}' is invalid.");
            }

            const int MaxPlaintextLength = 4096;

            if (request.Plaintext?.Length > MaxPlaintextLength)
            {
                return BadRequest($"The plaintext to hash cannot be more than {MaxPlaintextLength} characters in length.");
            }

            byte[] hash;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.ASCII, MaxPlaintextLength, true))
                {
                    writer.Write(request.Plaintext ?? string.Empty);
                    writer.Flush();
                }

                stream.Seek(0, SeekOrigin.Begin);

                using (var hasher = CreateHashAlgorithm(request.Algorithm))
                {
                    if (hasher == null)
                    {
                        return BadRequest($"The specified hash algorithm '{request.Algorithm}' is not supported.");
                    }

                    hash = hasher.ComputeHash(stream);
                }
            }

            var value = new HashResponse()
            {
                Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash),
            };

            return new OkObjectResult(value);
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
        [Route("tools/machinekey")]
        public IActionResult MachineKey([FromQuery]string decryptionAlgorithm, [FromQuery]string validationAlgorithm)
        {
            int decryptionKeyLength;
            int validationKeyLength;

            if (string.IsNullOrEmpty(decryptionAlgorithm) ||
                !HashSizes.TryGetValue(decryptionAlgorithm + "-D", out decryptionKeyLength))
            {
                return BadRequest($"The specified decryption algorithm '{decryptionAlgorithm}' is invalid.");
            }

            if (string.IsNullOrEmpty(validationAlgorithm) ||
                !HashSizes.TryGetValue(validationAlgorithm + "-V", out validationKeyLength))
            {
                return BadRequest($"The specified validation algorithm '{validationAlgorithm}' is invalid.");
            }

            byte[] decryptionKeyBytes = new byte[decryptionKeyLength];
            byte[] validationKeyBytes = new byte[validationKeyLength];

            var value = new MachineKeyResponse();

            try
            {
                using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(decryptionKeyBytes);
                }

                using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(validationKeyBytes);
                }

                value.DecryptionKey = BytesToHexString(decryptionKeyBytes).ToUpperInvariant();
                value.ValidationKey = BytesToHexString(validationKeyBytes).ToUpperInvariant();

                value.MachineKeyXml = string.Format(
                    CultureInfo.InvariantCulture,
                    @"<machineKey validationKey=""{0}"" decryptionKey=""{1}"" validation=""{2}"" decryption=""{3}"" />",
                    value.ValidationKey,
                    value.DecryptionKey,
                    validationAlgorithm,
                    decryptionAlgorithm);
            }
            finally
            {
                Array.Clear(decryptionKeyBytes, 0, decryptionKeyBytes.Length);
                Array.Clear(validationKeyBytes, 0, validationKeyBytes.Length);
            }

            return new OkObjectResult(value);
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing a hexadecimal representation of the specified <see cref="Array"/> of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to generate the hash string for.</param>
        /// <returns>
        /// A <see cref="string"/> containing the hexadecimal representation of <paramref name="buffer"/>.
        /// </returns>
        private static string BytesToHexString(byte[] buffer)
        {
            return string.Concat(buffer.Select((p) => p.ToString("x2", CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Creates a hash algorithm for the specified algorithm name.
        /// </summary>
        /// <param name="name">The name of the hash algorithm to create.</param>
        /// <returns>
        /// The created instance of <see cref="HashAlgorithm"/> if <paramref name="name"/>
        /// is valid; otherwise <see langword="null"/>.
        /// </returns>
        private static HashAlgorithm CreateHashAlgorithm(string name)
        {
            if (string.Equals(name, HashAlgorithmName.MD5.Name, StringComparison.OrdinalIgnoreCase))
            {
                return MD5.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA1.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA1.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA256.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA256.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA384.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA384.Create();
            }
            else if (string.Equals(name, HashAlgorithmName.SHA512.Name, StringComparison.OrdinalIgnoreCase))
            {
                return SHA512.Create();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a result that represents a bad API request.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that represents an invalid API request.
        /// </returns>
        private IActionResult BadRequest(string message)
        {
            var error = new ErrorResponse()
            {
                Message = message,
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = (int)HttpStatusCode.BadRequest,
            };

            return new BadRequestObjectResult(error);
        }
    }
}
