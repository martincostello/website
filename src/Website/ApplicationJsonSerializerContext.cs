// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MartinCostello.Website.Models;

namespace MartinCostello.Website;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(GuidResponse))]
[JsonSerializable(typeof(HashResponse))]
[JsonSerializable(typeof(MachineKeyResponse))]
internal sealed partial class ApplicationJsonSerializerContext : JsonSerializerContext
{
}
