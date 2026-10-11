// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using MartinCostello.Website.Options;
using Microsoft.Extensions.Options;

namespace MartinCostello.Website;

/// <summary>
/// A class containing endpoints for redirects. This class cannot be inherited.
/// </summary>
public static class RedirectsModule
{
    private static readonly string[] HttpMethods = ["GET", "HEAD", "POST"];

    private static readonly ImmutableDictionary<string, string[]> Redirects = ImmutableDictionary.CreateRange<string, string[]>(
    [
        new("https://blog.martincostello.com/", ["/blog", "/blog/{*catchall}"]),
        new("https://bsky.app/profile/martincostello.com", ["/bluesky", "/bsky", "/skeet"]),
        new("https://github.com/martincostello", ["/gh", "/github"]),
        new("https://github.com/martincostello/presentations", ["/presentations", "/slides", "/talks"]),
        new("https://github.com/sponsors/martincostello", ["/sponsor"]),
        new("https://mvp.microsoft.com/PublicProfile/5003438", ["/mvp"]),
        new("https://sessionize.com/martincostello/", ["/sessionize"]),
        new("https://stackoverflow.com/users/1064169/martin-costello", ["/so", "/stack", "/stackoverflow", "/stack-overflow"]),
        new("https://twitter.com/martin_costello", ["/tweet", "/tweets", "/twitter", "/x"]),
        new("https://www.linkedin.com/in/martin-costello/", ["/cv", "/in", "/linked-in", "/linkedin"]),
        new("https://www.youtube.com/playlist?list=PLG-3gLA2F6RiKi-H4E3-QroJGhn9iRTwU", ["/youtube"]),
    ]);

    /// <summary>
    /// Gets a random set of annoying YouTube videos. This field is read-only.
    /// </summary>
    /// <remarks>
    /// Inspired by <c>https://gist.github.com/NickCraver/c9458f2e007e9df2bdf03f8a02af1d13</c>.
    /// </remarks>
    private static readonly string[] Videos =
    [
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
    ];

    /// <summary>
    /// Maps the redirection routes.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to use.</param>
    /// <returns>
    /// The value of <paramref name="app"/>.
    /// </returns>
    public static IEndpointRouteBuilder MapRedirects(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<SiteOptions>>().Value;

        var browserStack = Results.Redirect(GetBrowserStackUrl(options));
        var blog = Results.Redirect(options.ExternalLinks?.Blog?.AbsoluteUri ?? "/");
        var securityTxt = Results.Redirect("/.well-known/security.txt");

        app.MapGet("/Content/browserstack.svg", () => browserStack);
        app.MapGet("/home/blog", () => blog);
        app.MapGet("/security.txt", () => securityTxt);

        foreach ((string url, string[] patterns) in Redirects)
        {
            var result = Results.Redirect(url);

            foreach (var pattern in patterns)
            {
                app.MapGet(pattern, () => result);
            }
        }

        var videos = new IResult[Videos.Length];

        for (int i = 0; i < videos.Length; i++)
        {
            videos[i] = Results.Redirect(Videos[i]);
        }

        foreach (string path in options.CrawlerPaths)
        {
#pragma warning disable CA5394
            app.MapMethods(path, HttpMethods, () => videos[Random.Shared.Next(videos.Length)]);
#pragma warning restore CA5394
        }

        return app;

        static string GetBrowserStackUrl(SiteOptions options)
        {
            var builder = new UriBuilder(options.ExternalLinks!.Cdn!)
            {
                Scheme = Uri.UriSchemeHttps,
                Path = "browserstack.svg",
            };

            return builder.Uri.ToString();
        }
    }
}
