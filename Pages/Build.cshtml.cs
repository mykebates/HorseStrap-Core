using HorseStrap.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorseStrap.Pages;

public class BuildModel : PageModel
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;

    public BuildModel(
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory)
    {
        _env = env;
        _httpClientFactory = httpClientFactory;

        OutputPath = Path.Combine(_env.ContentRootPath, "static");
        WebRoot = _env.WebRootPath;
    }

    [BindProperty]
    public string BaseUrl { get; set; } = string.Empty;

    [BindProperty]
    public string OutputPath { get; set; }

    [BindProperty]
    public string WebRoot { get; set; }

    [BindProperty]
    public string Exclusions { get; set; } = "_,Build,Error";

    public string? Message { get; private set; }

    public IReadOnlyList<StaticSiteGeneration.ExportResult> Results
    { get; private set; } = [];

    public void OnGet()
    {
        BaseUrl = $"{Request.Scheme}://{Request.Host}";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var pagesRoot = Path.Combine(_env.ContentRootPath, "Pages");

        var pages = StaticSiteGeneration.GetPages(
            pagesRoot,
            Exclusions.Split(',', StringSplitOptions.RemoveEmptyEntries));

        if (pages.Count == 0)
        {
            Message = $"No pages found under {pagesRoot}.";
            return Page();
        }

        StaticSiteGeneration.EnsureDirectories(OutputPath);

        // Copy wwwroot first so CSS, JS, fonts and images sit alongside
        // the HTML we're about to write.
        StaticSiteGeneration.CopyDirectory(WebRoot, OutputPath);

        var client = _httpClientFactory.CreateClient();
        var results = new List<StaticSiteGeneration.ExportResult>();

        foreach (var page in pages)
        {
            var route = StaticSiteGeneration.ToRoute(pagesRoot, page);

            results.Add(await StaticSiteGeneration.ExportPageAsync(
                client, BaseUrl, route, OutputPath, cancellationToken));
        }

        Results = results;

        var ok = results.Count(r => r.Success);
        Message = $"Exported {ok} of {results.Count} pages to {OutputPath}";

        return Page();
    }
}
