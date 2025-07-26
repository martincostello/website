﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;
using MartinCostello.Website.Benchmarks;

if (args.SequenceEqual(["--test"]))
{
    await using var benchmark = new WebsiteBenchmarks();
    await benchmark.StartServer();

    try
    {
        _ = await benchmark.Root();
        _ = await benchmark.Version();
    }
    finally
    {
        await benchmark.StopServer();
    }

    return 0;
}
else
{
    var summary = BenchmarkRunner.Run<WebsiteBenchmarks>(args: args);
    return summary.Reports.Any((p) => !p.Success) ? 1 : 0;
}
