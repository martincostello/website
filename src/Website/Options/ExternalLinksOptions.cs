// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;

namespace MartinCostello.Website.Options
{
    /// <summary>
    /// A class representing the external link options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class ExternalLinksOptions
    {
        /// <summary>
        /// Gets or sets the URI of the API.
        /// </summary>
        public Uri? Api { get; set; }

        /// <summary>
        /// Gets or sets the URI of the blog.
        /// </summary>
        public Uri? Blog { get; set; }

        /// <summary>
        /// Gets or sets the URI of the CDN.
        /// </summary>
        public Uri? Cdn { get; set; }

        /// <summary>
        /// Gets or sets the options for the URIs to use for reports.
        /// </summary>
        public ReportOptions? Reports { get; set; }
    }
}
