# Custom fonts

Drop your `.woff2` files in this directory.

## Quick start

1. Put the font file here, e.g. `wwwroot/fonts/inter-variable.woff2`
2. Open `wwwroot/css/fonts.css` and uncomment the matching `@font-face` block,
   fixing the family name and filename
3. Open `wwwroot/css/tokens.css` and put your family name at the front of the
   relevant stack:

   ```css
   --font-sans: "Inter", system-ui, sans-serif;
   ```

Save. `dotnet watch` hot-swaps the stylesheet without reloading the page.

## Only ship woff2

Every browser capable of running this app supports woff2, and it compresses
about 30% better than woff. You do not need `.eot`, `.ttf`, `.svg`, or `.woff`.
If a foundry hands you all five, use the woff2 and delete the rest.

## Converting fonts

If you were given `.ttf` or `.otf`:

```sh
# Homebrew
brew install woff2
woff2_compress YourFont.ttf     # produces YourFont.woff2
```

For subsetting (dropping glyphs you'll never render, often a 60–80% size win):

```sh
pipx install fonttools brotli
pyftsubset YourFont.ttf \
    --flavor=woff2 \
    --output-file=your-font.woff2 \
    --unicodes="U+0000-00FF,U+2000-206F,U+20AC,U+2122" \
    --layout-features="kern,liga,clig,calt"
```

## Prefer a variable font

One variable file usually beats three static cuts on total bytes, and gives you
every weight in between. Look for a `*-VariableFont*.woff2` or `*-VF.woff2` in
the download.

## Licensing

Confirm the license permits webfont embedding before self-hosting. Google Fonts
(OFL) and most open licenses are fine. Commercial foundry licenses frequently
are not, and are often priced by pageview.
