// Copyright (c) Martin Costello, 2016. All rights reserved.
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
        ["https://github.com/martincostello"] = ["/gh", "/github"],
        ["https://github.com/martincostello/presentations"] = ["/presentations", "/slides", "/talks"],
        ["https://stackoverflow.com/users/1064169/martin-costello"] = ["/so", "/stack", "/stackoverflow", "/stack-overflow"],
        ["https://twitter.com/martin_costello"] = ["/tweet", "/tweets", "/twitter", "/x"],
        ["https://www.linkedin.com/in/martin-costello/"] = ["/in", "/linked-in", "/linkedin"],
        ["https://www.youtube.com/martincostello"] = ["/youtube"],
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
        "https://www.youtube.com/watch?v=eh7lp9umG2I",
        "https://www.youtube.com/watch?v=z9Uz1icjwrM",
        "https://www.youtube.com/watch?v=Sagg08DrO5U",
        "https://www.youtube.com/watch?v=ER97mPHhgtM",
        "https://www.youtube.com/watch?v=jI-kpVh6e1U",
        "https://www.youtube.com/watch?v=jScuYd3_xdQ",
        "https://www.youtube.com/watch?v=S5PvBzDlZGs",
        "https://www.youtube.com/watch?v=9UZbGgXvCCA",
        "https://www.youtube.com/watch?v=O-dNDXUt1fg",
        "https://www.youtube.com/watch?v=MJ5JEhDy8nE",
        "https://www.youtube.com/watch?v=VnnWp_akOrE",
        "https://www.youtube.com/watch?v=jwGfwbsF4c4",
        "https://www.youtube.com/watch?v=8ZcmTl_1ER8",
        "https://www.youtube.com/watch?v=gLmcGkvJ-e0",
        "https://www.youtube.com/watch?v=ozPPwl53c_4",
        "https://www.youtube.com/watch?v=KMFOVSWn0mI",
        "https://www.youtube.com/watch?v=clU0Sh9ngmY",
        "https://www.youtube.com/watch?v=sCNrK-n68CM",
        "https://www.youtube.com/watch?v=hgwpZvTWLmE",
        "https://www.youtube.com/watch?v=CgBJ5irINqU",
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

        string[] crawlerPaths =
        [
            ".env",
            ".git/{*catchall}",
            "admin.php",
            "admin-console/{*catchall}",
            "admin/{*catchall}",
            "administration/{*catchall}",
            "administrator/{*catchall}",
            "appsettings.json",
            "ajaxproxy/{*catchall}",
            "bin/{*catchall}",
            "bitrix/admin/{*catchall}",
            "cms/{*catchall}",
            "index.php",
            "invoker/JMXInvokerServlet",
            "jmx-console/HtmlAdaptor",
            "license.php",
            "magmi/web/magmi.php",
            "manager/index.php",
            "modules/{*catchall}",
            "obj/{*catchall}",
            "package.json",
            "package-lock.json",
            "parameters.xml",
            "readme.htm",
            "readme.html",
            "site/{*catchall}",
            "sites/{*catchall}",
            "tiny_mce/{*catchall}",
            "uploadify/{*catchall}",
            "web.config",
            "web-console/Invoker",
            "wordpress/{*catchall}",
            "wp/{*catchall}",
            "wp-admin/{*catchall}",
            "wp-content/{*catchall}",
            "wp-includes/{*catchall}",
            "wp-links-opml.php",
            "wp-login.php",
            "xmlrpc.php",
        ];

        foreach (string path in crawlerPaths)
        {
            app.MapMethods(path, HttpMethods, RandomYouTubeVideo);
        }

        return app;

        static IResult RandomYouTubeVideo()
            => Results.Redirect(Videos[RandomNumberGenerator.GetInt32(0, Videos.Length)]);
    }
}
