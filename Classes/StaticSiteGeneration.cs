namespace HorseStrap.Classes;

/// <summary>
/// Crawls the app's own Razor Pages over HTTP and writes the rendered HTML
/// to disk, producing a static site you can host anywhere.
///
/// Rewritten from the original: WebClient (obsolete since .NET 6) is now
/// HttpClient, the synchronous file walk is async, and failures surface as
/// results instead of being swallowed into an unused local.
/// </summary>
public static class StaticSiteGeneration
{
    /// <summary>Outcome of exporting a single page.</summary>
    public record ExportResult(string Page, bool Success, string Message);

    /// <summary>
    /// Finds candidate .cshtml pages under <paramref name="pagesRoot"/>,
    /// skipping anything whose path contains one of
    /// <paramref name="exclusions"/> (case-insensitive).
    /// </summary>
    public static IReadOnlyList<string> GetPages(
        string pagesRoot,
        IEnumerable<string> exclusions)
    {
        if (!Directory.Exists(pagesRoot))
        {
            return [];
        }

        var skip = exclusions
            .Select(e => e.Trim())
            .Where(e => e.Length > 0)
            .ToArray();

        return Directory
            .EnumerateFiles(pagesRoot, "*.cshtml", SearchOption.AllDirectories)
            .Where(path =>
            {
                var relative = Path.GetRelativePath(pagesRoot, path);
                return !skip.Any(s =>
                    relative.Contains(s, StringComparison.OrdinalIgnoreCase));
            })
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Converts an absolute .cshtml path into the route it is served at.
    /// "Pages/About/Team.cshtml" becomes "About/Team"; Index becomes "".
    /// </summary>
    public static string ToRoute(string pagesRoot, string cshtmlPath)
    {
        var relative = Path.GetRelativePath(pagesRoot, cshtmlPath);

        // Normalize Windows separators to URL separators.
        relative = relative.Replace(Path.DirectorySeparatorChar, '/');

        if (relative.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            relative = relative[..^".cshtml".Length];
        }

        // "Index" is served at the directory root.
        if (relative.Equals("Index", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        if (relative.EndsWith("/Index", StringComparison.OrdinalIgnoreCase))
        {
            relative = relative[..^"/Index".Length];
        }

        return relative;
    }

    /// <summary>
    /// Maps a route to its output file, using the directory/index.html
    /// convention so URLs stay extensionless when served statically.
    /// </summary>
    public static string ToOutputPath(string outputRoot, string route)
    {
        return string.IsNullOrEmpty(route)
            ? Path.Combine(outputRoot, "index.html")
            : Path.Combine(
                outputRoot,
                route.Replace('/', Path.DirectorySeparatorChar),
                "index.html");
    }

    /// <summary>Ensures each directory exists, creating it if needed.</summary>
    public static void EnsureDirectories(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    /// <summary>
    /// Recursively copies <paramref name="source"/> into
    /// <paramref name="destination"/>, overwriting existing files.
    /// </summary>
    public static void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            return;
        }

        foreach (var dir in Directory.EnumerateDirectories(
                     source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(
                Path.Combine(destination, Path.GetRelativePath(source, dir)));
        }

        foreach (var file in Directory.EnumerateFiles(
                     source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(
                destination, Path.GetRelativePath(source, file));

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    /// <summary>
    /// Requests a single route and writes the response body to disk.
    /// </summary>
    public static async Task<ExportResult> ExportPageAsync(
        HttpClient client,
        string baseUrl,
        string route,
        string outputRoot,
        CancellationToken cancellationToken = default)
    {
        var label = string.IsNullOrEmpty(route) ? "/" : $"/{route}";
        var url = $"{baseUrl.TrimEnd('/')}/{route}";

        try
        {
            using var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ExportResult(
                    label, false, $"HTTP {(int)response.StatusCode}");
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var target = ToOutputPath(outputRoot, route);

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            await File.WriteAllTextAsync(target, html, cancellationToken);

            return new ExportResult(
                label, true, Path.GetRelativePath(outputRoot, target));
        }
        catch (Exception ex)
        {
            return new ExportResult(label, false, ex.Message);
        }
    }
}
