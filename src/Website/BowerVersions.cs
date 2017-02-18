// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.FileProviders;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A class that provides the versions of the Bower dependencies in use by the application. This class cannot be inherited.
    /// </summary>
    public class BowerVersions
    {
        /// <summary>
        /// The map of dependency names to versions. This field is read-only.
        /// </summary>
        private readonly IDictionary<string, string> _dependencies = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BowerVersions"/> class.
        /// </summary>
        /// <param name="environment">The <see cref="IHostingEnvironment"/> to use.</param>
        public BowerVersions(IHostingEnvironment environment)
        {
            IFileInfo path = environment.ContentRootFileProvider.GetFileInfo("bower.json");

            using (var stream = path.CreateReadStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    dynamic bower = JObject.Parse(text);

                    foreach (JProperty item in bower.dependencies)
                    {
                        _dependencies[item.Name] = (string)item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the version of the specified dependency name.
        /// </summary>
        /// <param name="name">The name of the dependency to get the version for.</param>
        /// <returns>
        /// The version of the specified dependency, if found; otherwise <see langword="null"/>.
        /// </returns>
        public string this[string name] => _dependencies[name];
    }
}
