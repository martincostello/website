// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website;

public static class TaskExtensions
{
    public static async Task<T2> ThenAsync<T1, T2>(this Task<T1> value, Func<T1, Task<T2>> continuation)
        => await continuation(await value);
}
