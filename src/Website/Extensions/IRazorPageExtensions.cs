// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Razor;

namespace MartinCostello.Website.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IRazorPage"/> interface. This class cannot be inherited.
/// </summary>
public static class IRazorPageExtensions
{
    /// <summary>
    /// Returns the current line number of the razor page.
    /// </summary>
    /// <param name="page">The razor page.</param>
    /// <param name="lineNumber">The optional current line number.</param>
    /// <returns>
    /// The current line number of the razor page.
    /// </returns>
    public static int CurrentLineNumber(this IRazorPage page, [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(page);
        return lineNumber;
    }
}
