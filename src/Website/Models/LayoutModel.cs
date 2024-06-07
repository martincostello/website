// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the page layout model. This class cannot be inherited.
/// </summary>
/// <param name="title">The page title.</param>
public sealed class LayoutModel(string title)
{
    /// <summary>
    /// Gets the page title.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Gets or sets the page description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the ROBOTS directive to use.
    /// </summary>
    public string? Robots { get; set; }
}
