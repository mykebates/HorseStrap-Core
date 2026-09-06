# HorseStrap

![HorseStrap](https://horsestrap.com/images/nily.svg)

A barebones ASP.NET Core starter kit. No build step, no bundler, no npm.

## Requirements

**.NET 10 SDK.** That's the whole list.

```sh
dotnet --version    # expect 10.x
```

Get it at <https://dotnet.microsoft.com/download>.

Node is *not* required. Neither is npm, webpack, Sass, or a task runner.

## Run it

```sh
dotnet watch
```

Opens <https://localhost:5001> with live reload wired up:

| You change | What happens |
| --- | --- |
| `wwwroot/css/*.css` | stylesheet hot-swaps, **no page reload** |
| `*.cshtml` | Razor hot reload |
| `wwwroot/js/*.js` | browser refreshes |
| `*.cs` | hot reload, or rebuild + restart if the edit can't be applied live |

`dotnet run` also works if you don't want the watcher.

## What's in the box

| | |
| --- | --- |
| **.NET 10** | Razor Pages, minimal hosting |
| **Plain CSS** | cascade layers, custom properties, native nesting |
| **Alpine.js 3.17** | ~15KB, declarative interactivity |
| **FontAwesome 7** | self-hosted, woff2 only |
| **Static export** | render the whole site to flat HTML |

Zero NuGet packages. Zero npm packages.

## Layout

```
Pages/              Razor Pages
  _Layout.cshtml    shell — the only place assets are referenced
  Index.cshtml      home page (a live demo of the stack — delete it)
  Build.cshtml      static site generator UI
Classes/            supporting C#
wwwroot/
  css/              your stylesheets (see below)
  js/main.js        your JS entry point
  fonts/            drop custom .woff2 files here
  lib/              vendored third-party (Alpine, FontAwesome)
```

## Styles

Everything lives in `wwwroot/css/`. What you edit is what the browser gets —
there is no compile step and no source maps to reason about.

`main.css` is the only file referenced by the layout. It declares the cascade
layer order and imports the rest:

```css
@layer reset, tokens, fonts, base, layout, modules, utilities, pages;
```

| File | What goes in it |
| --- | --- |
| `reset.css` | modern baseline, replaces normalize |
| `tokens.css` | **all design decisions** — color, type, spacing, dark mode |
| `fonts.css` | `@font-face` declarations |
| `base.css` | bare element styling |
| `layout.css` | container, stack, cluster, grid, sidebar primitives |
| `modules.css` | your components — most CSS ends up here |
| `utilities.css` | single-purpose helpers |
| `pages.css` | one-off page styling |

### Why layers

Later layers beat earlier ones **regardless of selector specificity**. A
`.text-center` utility overrides a `.card h2` rule without `!important` and
without specificity games. Put a rule in the right layer and it wins.

Anything outside a layer beats everything inside one, which is your escape
hatch.

### Coming from the Sass version

| Sass | Now |
| --- | --- |
| `$purple` | `var(--purple-500)` in `tokens.css` |
| `&:hover { }` | same — native CSS nesting |
| `@include mq-medium { }` | `@media (width < 48rem) { }` |
| `@import "partial"` | `@import url("partial.css") layer(x)` |
| `_normalize.scss` | `reset.css` |
| `.container.wide` | unchanged |

Custom properties beat Sass variables in one important way: they exist at
runtime. Change one in DevTools and the page reacts. Dark mode is a dozen
reassignments in a `prefers-color-scheme` block rather than a second
compiled theme.

## JavaScript

`wwwroot/js/main.js` is an ES module, loaded deferred.

Most interactivity needs no JS file at all — Alpine reads directives straight
off your markup:

```html
<div x-data="{ open: false }" @click.outside="open = false">
    <button @click="open = !open" :aria-expanded="open">Menu</button>
    <ul x-show="open" x-transition x-cloak>
        <li><a href="/one">One</a></li>
    </ul>
</div>
```

`x-data` state · `x-model` binding · `x-for` loops · `x-show`/`x-if`
conditionals · `x-on` events · `x-transition` animation.

For logic too big for an attribute, register a component in `main.js` with
`Alpine.data()` and apply it with `x-data="name"`. Three examples ship in
that file. Global state goes in `Alpine.store()`.

Docs: <https://alpinejs.dev>

> **Razor gotcha:** `@` starts a C# expression in `.cshtml`. Write `@@click`
> to emit a literal `@click`, or use the longhand `x-on:click`.

## Icons

FontAwesome 7 Free, self-hosted:

```html
<i class="fa-solid fa-horse-head"></i>
<i class="fa-regular fa-star"></i>
<i class="fa-brands fa-github"></i>
```

Browse at <https://fontawesome.com/search?o=r&m=free>. Not using icons? Delete
`wwwroot/lib/fontawesome/` and its `<link>` in `_Layout.cshtml`.

## Updating Alpine and FontAwesome

Both are vendored into `wwwroot/lib/`. One script fetches them:

```sh
./scripts/vendor.sh              # reinstall the pinned versions
./scripts/vendor.sh --latest     # bump to latest and re-pin
./scripts/vendor.sh alpine       # just one of them
```

Versions are pinned at the top of the script. `--latest` resolves from the
npm registry and rewrites those two lines, so a bump shows up as a real diff
instead of a number you have to remember.

The script also does the two things that are easy to forget by hand: it keeps
FontAwesome to `.woff2` only, and rewrites the stock `../webfonts/` paths to
`webfonts/`, since we nest the CSS and fonts together rather than as siblings.

Nothing here runs at build time — it's a deliberate, occasional action that
commits real files.

## Custom fonts

1. Drop a `.woff2` into `wwwroot/fonts/`
2. Uncomment the matching `@font-face` in `wwwroot/css/fonts.css`
3. Put the family name at the front of `--font-sans` in `tokens.css`

`fonts.css` has ready-to-use blocks for variable fonts, static weights,
`unicode-range` subsetting, and metric-override fallbacks that eliminate
layout shift on font swap. See `wwwroot/fonts/README.md` for conversion and
subsetting commands.

Defaults are system font stacks, so a fresh clone renders instantly with no
font downloads.

## Static site export

Visit `/Build` while the app is running. It crawls every Razor page over HTTP,
writes the rendered HTML to `static/`, and copies `wwwroot` alongside it.
Upload that folder anywhere.

Pages matching the exclusion list are skipped. `Index` maps to `index.html`;
everything else gets `route/index.html` so URLs stay extensionless.

## Deploy

```sh
dotnet publish -c Release
```

Output lands in `bin/Release/net10.0/publish/`.

## License

MIT
