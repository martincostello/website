﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Options;

namespace MartinCostello.Website.Models;

/// <summary>
/// A class representing the view model for page metadata. This class cannot be inherited.
/// </summary>
public sealed class MetaModel
{
    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the Bitcoin address of the author.
    /// </summary>
    public string? Bitcoin { get; set; }

    /// <summary>
    /// Gets or sets the canonical URI.
    /// </summary>
    public string? CanonicalUri { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Facebook profile Id.
    /// </summary>
    public string? Facebook { get; set; }

    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets the image URI.
    /// </summary>
    public string? ImageUri { get; set; }

    /// <summary>
    /// Gets or sets the image alternate text.
    /// </summary>
    public string? ImageAltText { get; set; }

    /// <summary>
    /// Gets or sets the page keywords.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets the robots value.
    /// </summary>
    public string? Robots { get; set; }

    /// <summary>
    /// Gets or sets the site name.
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// Gets or sets the site type.
    /// </summary>
    public string? SiteType { get; set; }

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the Twitter card type.
    /// </summary>
    public string? TwitterCard { get; set; }

    /// <summary>
    /// Gets or sets the Twitter handle.
    /// </summary>
    public string? TwitterHandle { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MetaModel"/>.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <param name="canonicalUri">The optional canonical URI of the page.</param>
    /// <param name="description">The optional page description.</param>
    /// <param name="imageUri">The optional image URI.</param>
    /// <param name="imageAltText">The optional image alternate text.</param>
    /// <param name="robots">The optional robots value.</param>
    /// <param name="title">The optional page title.</param>
    /// <returns>
    /// The created instance of <see cref="MetaModel"/>.
    /// </returns>
    public static MetaModel Create(
        MetadataOptions? options,
        string? canonicalUri = null,
        string? description = null,
        string? imageUri = null,
        string? imageAltText = null,
        string? robots = null,
        string? title = null)
    {
        options ??= new MetadataOptions();

        return new MetaModel()
        {
            Author = options.Author?.Name,
            Bitcoin = options.Author?.Bitcoin,
            CanonicalUri = canonicalUri ?? string.Empty,
            Description = description ?? options.Description,
            Facebook = options.Author?.SocialMedia?.Facebook,
            HostName = options.Domain,
            ImageUri = imageUri ?? string.Empty,
            ImageAltText = imageAltText ?? string.Empty,
            Keywords = options.Keywords ?? "martin,costello,website",
            Robots = robots ?? options.Robots,
            SiteName = options.Name ?? "Personal Website of Martin Costello",
            SiteType = options.Type ?? "website",
            Title = $"{title} - {options.Name}",
            TwitterCard = "summary",
            TwitterHandle = options.Author?.SocialMedia?.Twitter,
        };
    }
}
