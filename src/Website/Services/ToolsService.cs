// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text;
using MartinCostello.Website.Models;

namespace MartinCostello.Website.Services;

/// <summary>
/// A class representing the default implementation of <see cref="IToolsService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ToolsService"/> class.
/// </remarks>
/// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to use.</param>
public class ToolsService(IHttpContextAccessor contextAccessor) : IToolsService
{
    /// <summary>
    /// An <see cref="IDictionary{K, V}"/> containing the sizes of the decryption and validation hashes for machine keys.
    /// </summary>
    private static readonly FrozenDictionary<string, int> HashSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["3DES-D"] = 24,
        ["3DES-V"] = 24,
        ["AES-128-D"] = 16,
        ["AES-192-D"] = 24,
        ["AES-256-D"] = SHA256.HashSizeInBytes,
        ["AES-V"] = 32,
        ["DES-D"] = 32,
        ["MD5-V"] = MD5.HashSizeInBytes,
        ["HMACSHA256-V"] = SHA256.HashSizeInBytes,
        ["HMACSHA384-V"] = SHA384.HashSizeInBytes,
        ["HMACSHA512-V"] = SHA512.HashSizeInBytes,
        ["SHA1-V"] = 64,
    }.ToFrozenDictionary();

    /// <summary>
    /// The HttpContext accessor This field is read-only.
    /// </summary>
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    /// <inheritdoc/>
    public IResult GenerateGuid(string? format, bool? uppercase)
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

        var result = new GuidResponse()
        {
            Guid = guid,
        };

        return Results.Json(result, ApplicationJsonSerializerContext.Default.GuidResponse);
    }

    /// <inheritdoc/>
    public IResult GenerateHash(HashRequest request)
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

        if (request.Plaintext?.Length > MaxPlaintextLength)
        {
            return BadRequest($"The plaintext to hash cannot be more than {MaxPlaintextLength} characters in length.");
        }

        byte[] buffer = Encoding.UTF8.GetBytes(request.Plaintext ?? string.Empty);
        HashAlgorithmName? hashAlgorithm = request.Algorithm.ToUpperInvariant() switch
        {
            "MD5" => HashAlgorithmName.MD5,
            "SHA1" => HashAlgorithmName.SHA1,
            "SHA256" => HashAlgorithmName.SHA256,
            "SHA384" => HashAlgorithmName.SHA384,
            "SHA512" => HashAlgorithmName.SHA512,
            _ => null,
        };

        if (hashAlgorithm is not { } algorithm)
        {
            return BadRequest($"The specified hash algorithm '{request.Algorithm}' is not supported.");
        }

        byte[] hash = CryptographicOperations.HashData(algorithm, buffer);

        var result = new HashResponse()
        {
            Hash = formatAsBase64 ? Convert.ToBase64String(hash) : BytesToHexString(hash, toLower: true),
        };

        return Results.Json(result, ApplicationJsonSerializerContext.Default.HashResponse);
    }

    /// <inheritdoc/>
    public IResult GenerateMachineKey(string? decryptionAlgorithm, string? validationAlgorithm)
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

        byte[] decryptionKey = RandomNumberGenerator.GetBytes(decryptionKeyLength);
        byte[] validationKey = RandomNumberGenerator.GetBytes(validationKeyLength);

        var result = new MachineKeyResponse()
        {
            DecryptionKey = BytesToHexString(decryptionKey),
            ValidationKey = BytesToHexString(validationKey),
        };

        result.MachineKeyXml = string.Format(
            CultureInfo.InvariantCulture,
            @"<machineKey validationKey=""{0}"" decryptionKey=""{1}"" validation=""{2}"" decryption=""{3}"" />",
            result.ValidationKey,
            result.DecryptionKey,
            validationAlgorithm.Split('-', StringSplitOptions.RemoveEmptyEntries)[0].ToUpperInvariant(),
            decryptionAlgorithm.Split('-', StringSplitOptions.RemoveEmptyEntries)[0].ToUpperInvariant());

        return Results.Json(result, ApplicationJsonSerializerContext.Default.MachineKeyResponse);
    }

    /// <summary>
    /// Returns a <see cref="string"/> containing a hexadecimal representation of the specified <see cref="ReadOnlySpan{T}"/> of bytes.
    /// </summary>
    /// <param name="bytes">The buffer to generate the hash string for.</param>
    /// <param name="toLower">Whether to return the hash in lowercase.</param>
    /// <returns>
    /// A <see cref="string"/> containing the hexadecimal representation of <paramref name="bytes"/>.
    /// </returns>
    private static string BytesToHexString(ReadOnlySpan<byte> bytes, bool toLower = false)
        => toLower ? Convert.ToHexStringLower(bytes) : Convert.ToHexString(bytes);

    /// <summary>
    /// Returns a result that represents a bad API request.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>
    /// An <see cref="IResult"/> that represents an invalid API request.
    /// </returns>
    private IResult BadRequest(string message)
    {
        var error = new ErrorResponse()
        {
            Message = message,
            RequestId = _contextAccessor.HttpContext!.TraceIdentifier,
            StatusCode = StatusCodes.Status400BadRequest,
        };

        return Results.Json(error, ApplicationJsonSerializerContext.Default.ErrorResponse, statusCode: error.StatusCode);
    }
}
