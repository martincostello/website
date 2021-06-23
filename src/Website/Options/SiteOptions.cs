// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MartinCostello.Website.Options
{
    /// <summary>
    /// A class representing the site configuration. This class cannot be inherited.
    /// </summary>
    public sealed class SiteOptions
    {
        /// <summary>
        /// Gets or sets the analytics options for the site.
        /// </summary>
        public AnalyticsOptions? Analytics { get; set; }

        /// <summary>
        /// Gets or sets the certificate transparency options to use.
        /// </summary>
        public CertificateTransparencyOptions? CertificateTransparency { get; set; }

        /// <summary>
        /// Gets or sets the Content Security Policy origins for the site.
        /// </summary>
        public IDictionary<string, IList<string>>? ContentSecurityPolicyOrigins { get; set; }

        /// <summary>
        /// Gets or setsht the external link options for the site.
        /// </summary>
        public ExternalLinksOptions? ExternalLinks { get; set; }

        /// <summary>
        /// Gets or sets the metadata options for the site.
        /// </summary>
        public MetadataOptions? Metadata { get; set; }
    }
}
