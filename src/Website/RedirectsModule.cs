﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using MartinCostello.Website.Options;
using Microsoft.Extensions.Options;

namespace MartinCostello.Website;

/// <summary>
/// A class containing endpoints for redirects. This class cannot be inherited.
/// </summary>
public static class RedirectsModule
{
    private static readonly string[] HttpMethods = ["GET", "HEAD", "POST"];

    private static readonly Dictionary<string, string[]> Redirects = new()
    {
        ["https://blog.martincostello.com/"] = ["/blog", "/blog/{*catchall}"],
        ["https://bsky.app/profile/martincostello.com"] = ["/bluesky", "/bsky", "/skeet"],
        ["https://github.com/martincostello"] = ["/gh", "/github"],
        ["https://github.com/martincostello/presentations"] = ["/presentations", "/slides", "/talks"],
        ["https://github.com/sponsors/martincostello"] = ["/sponsor"],
        ["https://mvp.microsoft.com/PublicProfile/5003438"] = ["/mvp"],
        ["https://sessionize.com/martincostello/"] = ["/sessionize"],
        ["https://stackoverflow.com/users/1064169/martin-costello"] = ["/so", "/stack", "/stackoverflow", "/stack-overflow"],
        ["https://twitter.com/martin_costello"] = ["/tweet", "/tweets", "/twitter", "/x"],
        ["https://www.linkedin.com/in/martin-costello/"] = ["/cv", "/in", "/linked-in", "/linkedin"],
        ["https://www.youtube.com/playlist?list=PLG-3gLA2F6RiKi-H4E3-QroJGhn9iRTwU"] = ["/youtube"],
    };

    /// <summary>
    /// Gets a random set of annoying YouTube videos. This field is read-only.
    /// </summary>
    /// <remarks>
    /// Inspired by <c>https://gist.github.com/NickCraver/c9458f2e007e9df2bdf03f8a02af1d13</c>.
    /// </remarks>
    private static ReadOnlySpan<string> Videos => new[]
    {
        "https://www.youtube.com/watch?v=wbby9coDRCk",
        "https://www.youtube.com/watch?v=nb2evY0kmpQ",
        "https://www.youtube.com/watch?v=z9Uz1icjwrM",
        "https://www.youtube.com/watch?v=Sagg08DrO5U",
        "https://www.youtube.com/watch?v=jScuYd3_xdQ",
        "https://www.youtube.com/watch?v=S5PvBzDlZGs",
        "https://www.youtube.com/watch?v=9UZbGgXvCCA",
        "https://www.youtube.com/watch?v=O-dNDXUt1fg",
        "https://www.youtube.com/watch?v=MJ5JEhDy8nE",
        "https://www.youtube.com/watch?v=VnnWp_akOrE",
        "https://www.youtube.com/watch?v=sCNrK-n68CM",
        "https://www.youtube.com/watch?v=hgwpZvTWLmE",
        "https://www.youtube.com/watch?v=jAckVuEY_Rc",
    };

    /// <summary>
    /// Maps the redirection routes.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to use.</param>
    /// <returns>
    /// The value of <paramref name="app"/>.
    /// </returns>
    public static IEndpointRouteBuilder MapRedirects(this IEndpointRouteBuilder app)
    {
        app.MapGet("/Content/browserstack.svg", (IOptions<SiteOptions> options) =>
        {
            var builder = new UriBuilder(options.Value!.ExternalLinks!.Cdn!)
            {
                Scheme = Uri.UriSchemeHttps,
                Path = "browserstack.svg",
            };

            return Results.Redirect(builder.Uri.ToString());
        });

        app.MapGet("/home/blog", (IOptions<SiteOptions> options) =>
        {
            return Results.Redirect(options.Value?.ExternalLinks?.Blog?.AbsoluteUri ?? "/");
        });

        foreach ((string url, string[] patterns) in Redirects)
        {
            foreach (var pattern in patterns)
            {
                app.MapGet(pattern, () => Results.Redirect(url));
            }
        }

        var options = app.ServiceProvider.GetRequiredService<IOptions<SiteOptions>>();

        foreach (string path in options.Value.CrawlerPaths)
        {
            app.MapMethods(path, HttpMethods, RandomYouTubeVideo);
        }

        return app;

        static IResult RandomYouTubeVideo()
            => Results.Redirect(Videos[RandomNumberGenerator.GetInt32(0, Videos.Length)]);
    }
}
