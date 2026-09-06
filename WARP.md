# WARP.md — HorseStrap Core

## Purpose

HorseStrap is a barebones ASP.NET Core starter kit — the skeleton you clone
when starting a new site and don't want to fight a toolchain. Priority is
minimum dependencies and zero build step, not features.

## Tech stack

- **.NET 10** — Razor Pages, minimal hosting (`Program.cs`, no `Startup.cs`)
- **Plain CSS** — cascade layers, custom properties, native nesting
- **Alpine.js 3.17.1** — vendored, `wwwroot/lib/alpine/`
- **FontAwesome Free 7.3.1** — vendored, `wwwroot/lib/fontawesome/`, woff2 only

Vendored versions are pinned in `scripts/vendor.sh`, which is the only place
they are declared. Update them with that script, not by hand.

Zero NuGet packages. Zero npm packages. No `package.json` — if one appears,
something went wrong.

## Current state

Fully modernized from the original 2018 build (Sept 2026). Builds and runs
clean. Static export verified working.

**What changed from the original:**

| Was | Now |
| --- | --- |
| netcoreapp2.1 + `Startup.cs` | net10.0 + minimal hosting |
| node-sass + webpack + BrowserSync | nothing — CSS ships as-authored |
| webpack HMR on `:3000` proxying `:5001` | `dotnet watch` browser refresh on `:5001` |
| jQuery + fancyBox + SweetAlert + animateCSS | Alpine.js |
| FontAwesome 5 Sass port, 5 font formats | FontAwesome 7, woff2 only |
| `assets/sass/**` → compiled to `wwwroot/css/` | authored directly in `wwwroot/css/` |
| `?horse_build` query-string CSS hack | removed — CSS is always a real file |
| `WebClient`, `IHostingEnvironment` | `HttpClient`, `IWebHostEnvironment` |
| namespaces `CareToLearnUI` / `MethodConf` | `HorseStrap` |

12 npm devDependencies → 0. 1 NuGet package → 0.

## Commands

```sh
dotnet watch                 # dev server + live reload, https://localhost:5001
dotnet run                   # no watcher
dotnet build                 # compile
dotnet publish -c Release    # → bin/Release/net10.0/publish/

./scripts/vendor.sh          # reinstall pinned Alpine + FontAwesome
./scripts/vendor.sh --latest # bump both to latest, re-pin in the script
```

`vendor.sh` is idempotent — running it without `--latest` leaves the working
tree clean. It handles the woff2-only trim and the `../webfonts/` →
`webfonts/` path rewrite that FontAwesome otherwise requires by hand.

Static site export: run the app, visit `/Build`, submit. Output → `static/`.

## Structure

```
Program.cs              minimal hosting, ~25 lines
HorseStrap.csproj       net10.0, no PackageReferences
scripts/
  vendor.sh             fetches + patches vendored Alpine and FontAwesome
Classes/
  StaticSiteGeneration.cs   crawl + export helpers
Pages/
  _Layout.cshtml        only place assets are referenced
  Index.cshtml          demo of the stack — delete when building for real
  Build.cshtml(.cs)     static export UI
wwwroot/
  css/                  main + 7 layer files, authored in place
  js/main.js            ES module, registers Alpine components
  fonts/                drop .woff2 here
  lib/                  vendored Alpine + FontAwesome
```

## Conventions

- **4-space indentation** everywhere (C#, CSS, JS, Razor)
- CSS goes in the layer that matches its job; never reach for `!important`
- All design values belong in `tokens.css` — no magic numbers in components
- Any region with a **hard-coded background** must reassign the semantic
  colour aliases it sits on (and set `color-scheme`), not just its own
  `color`. Children read the tokens directly, so setting `color` alone
  leaves them resolving against the visitor's scheme. See `.page-home`.
- Prefer Alpine directives in markup over JS files
- In `.cshtml`, escape Alpine's `@` as `@@` (`@@click`) or use `x-on:click`

## Known issues

None outstanding.

## Next steps

Whatever the next project needs. Suggested starting points:

- Delete the demo content in `Index.cshtml` and the `.page-home` block in
  `pages.css`
- Add an `Error.cshtml` page (`Program.cs` already routes to `/Error` in
  production, but the page does not exist yet)
- Split `modules.css` into `wwwroot/css/modules/*.css` once it grows
- Wire a real font into `fonts.css`
