// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Services
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Api.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Options;

    /// <summary>
    /// A class representing the default implementation of <see cref="IToolsService"/>.
    /// </summary>
    public class ToolsService : IToolsService
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
        /// The trace Id of the current request. This field is read-only.
        /// </summary>
        private readonly string _traceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolsService"/> class.
        /// </summary>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to use.</param>
        /// <param name="options">The site options to use.</param>
        public ToolsService(IHttpContextAccessor contextAccessor, IOptions<SiteOptions> options)
        {
            _apiUri = options?.Value?.ExternalLinks?.Api;
            _traceId = contextAccessor.HttpContext.TraceIdentifier;
        }

        /// <inheritdoc/>
        public ActionResult<GuidResponse> GenerateGuid(string format, bool? uppercase)
        {
            string guid;

            try
            {
                guid = Guid.NewGuid().ToString(format ?? "D", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return BadRequest($"The specified format '{format}' is invalid.");
            }

            if (uppercase == true)
            {
                guid = guid.ToUpperInvariant();
            }

            return new GuidResponse()
            {
                Guid = guid,
            };
        }

        /// <inheritdoc/>
        public async Task<ActionResult<HashResponse>> GenerateHashAsync(HashRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Algorithm))
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
                    await writer.WriteAsync(request.Plaintext ?? string.Empty).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
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

            return new HashResponse()
            {
                Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash),
            };
        }

        /// <inheritdoc/>
        public ActionResult<MachineKeyResponse> GenerateMachineKey(string decryptionAlgorithm, string validationAlgorithm)
        {
            if (string.IsNullOrEmpty(decryptionAlgorithm) ||
                !HashSizes.TryGetValue(decryptionAlgorithm + "-D", out int decryptionKeyLength))
            {
                return BadRequest($"The specified decryption algorithm '{decryptionAlgorithm}' is invalid.");
            }

            if (string.IsNullOrEmpty(validationAlgorithm) ||
                !HashSizes.TryGetValue(validationAlgorithm + "-V", out int validationKeyLength))
            {
                return BadRequest($"The specified validation algorithm '{validationAlgorithm}' is invalid.");
            }

            var pool = ArrayPool<byte>.Shared;
            var decryptionKeyBytes = pool.Rent(decryptionKeyLength);
            var validationKeyBytes = pool.Rent(validationKeyLength);

            try
            {
                var decryptionKey = decryptionKeyBytes.AsSpan(0, decryptionKeyLength);
                var validationKey = validationKeyBytes.AsSpan(0, validationKeyLength);

                using (var random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(decryptionKey);
                    random.GetBytes(validationKey);
                }

                var result = new MachineKeyResponse()
                {
                    DecryptionKey = BytesToHexString(decryptionKey).ToUpperInvariant(),
                    ValidationKey = BytesToHexString(validationKey).ToUpperInvariant(),
                };

                result.MachineKeyXml = string.Format(
                    CultureInfo.InvariantCulture,
                    @"<machineKey validationKey=""{0}"" decryptionKey=""{1}"" validation=""{2}"" decryption=""{3}"" />",
                    result.ValidationKey,
                    result.DecryptionKey,
                    validationAlgorithm,
                    decryptionAlgorithm);

                return result;
            }
            finally
            {
                pool.Return(decryptionKeyBytes, true);
                pool.Return(validationKeyBytes, true);
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing a hexadecimal representation of the specified <see cref="Array"/> of bytes.
        /// </summary>
        /// <param name="span">The buffer to generate the hash string for.</param>
        /// <returns>
        /// A <see cref="string"/> containing the hexadecimal representation of <paramref name="span"/>.
        /// </returns>
        private static string BytesToHexString(ReadOnlySpan<byte> span)
        {
            var format = new StringBuilder(span.Length);

            foreach (var b in span)
            {
                format.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return format.ToString();
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
#pragma warning disable CA5351 // Do not use insecure cryptographic algorithm MD5.
                return MD5.Create();
#pragma warning restore CA5351 // Do not use insecure cryptographic algorithm MD5.
            }
            else if (string.Equals(name, HashAlgorithmName.SHA1.Name, StringComparison.OrdinalIgnoreCase))
            {
#pragma warning disable CA5350 // Do not use insecure cryptographic algorithm SHA1.
                return SHA1.Create();
#pragma warning restore CA5350 // Do not use insecure cryptographic algorithm SHA1.
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
        /// An <see cref="ActionResult"/> that represents an invalid API request.
        /// </returns>
        private ActionResult BadRequest(string message)
        {
            var error = new ErrorResponse()
            {
                Message = message,
                RequestId = _traceId,
                StatusCode = StatusCodes.Status400BadRequest,
            };

            return new BadRequestObjectResult(error);
        }
    }
}
