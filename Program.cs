var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Used by the static site generator to crawl its own pages.
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

// Plain UseStaticFiles rather than MapStaticAssets: the fingerprinted URLs
// MapStaticAssets emits would not resolve in the flat file copy that the
// static site generator produces.
app.UseStaticFiles();

app.MapRazorPages();

app.Run();
