// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.Options
{
    using System;

    /// <summary>
    /// A class representing the external link options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class ExternalLinksOptions
    {
        /// <summary>
        /// Gets or sets the URI of the API.
        /// </summary>
        public Uri Api { get; set; }

        /// <summary>
        /// Gets or sets the URI of the blog.
        /// </summary>
        public Uri Blog { get; set; }

        /// <summary>
        /// Gets or sets the URI of the status website.
        /// </summary>
        public Uri Status { get; set; }
    }
}
