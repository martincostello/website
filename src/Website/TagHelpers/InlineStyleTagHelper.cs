﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.TagHelpers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.FileProviders;

    /// <summary>
    /// A <see cref="ITagHelper"/> implementation targeting &lt;link&gt;
    /// elements that supports inlining styles for local CSS files.
    /// </summary>
    [HtmlTargetElement("link", Attributes = InlineAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("link", Attributes = MinifyInlinedAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class InlineStyleTagHelper : LinkTagHelper
    {
        /// <summary>
        /// The name of the <c>asp-inline</c> attribute.
        /// </summary>
        private const string InlineAttributeName = "asp-inline";

        /// <summary>
        /// The name of the <c>asp-inline</c> attribute.
        /// </summary>
        private const string MinifyInlinedAttributeName = "asp-minify-inlined";

        /// <summary>
        /// An array containing the <see cref="Environment.NewLine"/> string.
        /// </summary>
        private static readonly string[] NewLine = new[] { Environment.NewLine };

        /// <summary>
        /// An array containing the <c>~</c> character.
        /// </summary>
        private static readonly char[] Tilde = new[] { '~' };

        public InlineStyleTagHelper(IHostingEnvironment hostingEnvironment, IMemoryCache cache, HtmlEncoder htmlEncoder, JavaScriptEncoder javaScriptEncoder, IUrlHelperFactory urlHelperFactory)
            : base(hostingEnvironment, cache, htmlEncoder, javaScriptEncoder, urlHelperFactory)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether CSS should be inlined.
        /// </summary>
        [HtmlAttributeName(InlineAttributeName)]
        public bool? Inline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inlined CSS should be minified.
        /// </summary>
        [HtmlAttributeName(MinifyInlinedAttributeName)]
        public bool? MinifyInlined { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            bool shouldProcess =
                Inline == true &&
                string.Equals(context.AllAttributes["rel"]?.Value?.ToString() as string, "stylesheet", StringComparison.OrdinalIgnoreCase);

            if (!shouldProcess)
            {
                // Not enabled or not a stylesheet
                await base.ProcessAsync(context, output);
                return;
            }

            string filePath = (context.AllAttributes["href"].Value as string)?.TrimStart(Tilde);
            IFileInfo fileInfo = HostingEnvironment.WebRootFileProvider.GetFileInfo(filePath);

            if (!fileInfo.Exists)
            {
                // Not a local file
                await base.ProcessAsync(context, output);
                return;
            }

            string css;
            string cacheKey = $"inline-css-{fileInfo.PhysicalPath}-{MinifyInlined == true}";

            if (!Cache.TryGetValue(cacheKey, out css))
            {
                using (var stream = File.OpenRead(fileInfo.PhysicalPath))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        css = await reader.ReadToEndAsync();
                    }
                }

                if (MinifyInlined == true)
                {
                    css = MinifyCss(css);
                }

                Cache.Set(cacheKey, css);
            }

            output.Content.SetHtmlContent(css);

            output.Attributes.Clear();

            output.TagName = "style";
            output.TagMode = TagMode.StartTagAndEndTag;
        }

        /// <summary>
        /// Naively minify the CSS in the specified string.
        /// </summary>
        /// <param name="css">A string containing CSS to minfiy.</param>
        /// <returns>
        /// A string containing the minified representation of <paramref name="css"/>.
        /// </returns>
        private static string MinifyCss(string css)
        {
            // Remove all blank lines, trim space between line contents and turn into a single line
            string[] lines = css.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
            string minified = string.Join(string.Empty, lines.Select((p) => p.Trim()));

            var builder = new StringBuilder(minified);

            for (int i = 0; i < builder.Length - 1; i++)
            {
                char ch = builder[i];

                // Remove spaces before element starts, such as: "body {...}" => "body{...}"
                // Remove spaces after delimited lists, such as: "font: a, b, 'c d'" => "font: a,b,'c d'"
                // Remove spaces after value starts, such as: "resize: none;" => "resize:none;"
                if (ch == '{')
                {
                    int previous = i - 1;

                    if (builder[previous] == ' ')
                    {
                        builder.Remove(previous, 1);
                    }
                }
                else if (ch == ',' || ch == ':')
                {
                    int next = i + 1;

                    if (builder[next] == ' ')
                    {
                        builder.Remove(next, 1);
                    }
                }
            }

            return builder.ToString();
        }
    }
}
