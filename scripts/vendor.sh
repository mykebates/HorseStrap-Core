#!/usr/bin/env bash
#
# Refreshes the vendored front-end libraries in wwwroot/lib/.
#
#   ./scripts/vendor.sh                 # install the pinned versions below
#   ./scripts/vendor.sh --latest        # resolve latest from the npm registry
#   ./scripts/vendor.sh alpine          # only one library
#   ./scripts/vendor.sh --latest alpine
#
# Pinned versions are the single source of truth. When you bump with --latest,
# the script rewrites these two lines so the change lands in your diff.

set -euo pipefail

ALPINE_VERSION="3.17.1"
FONTAWESOME_VERSION="7.3.1"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LIB="$ROOT/wwwroot/lib"
SELF="${BASH_SOURCE[0]}"

USE_LATEST=0
TARGETS=()

for arg in "$@"; do
    case "$arg" in
        --latest)
            USE_LATEST=1
            ;;
        alpine|fontawesome)
            TARGETS+=("$arg")
            ;;
        -h|--help)
            sed -n '2,11p' "$SELF" | cut -c3-
            exit 0
            ;;
        *)
            echo "unknown argument: $arg" >&2
            exit 1
            ;;
    esac
done

if [ ${#TARGETS[@]} -eq 0 ]; then
    TARGETS=(alpine fontawesome)
fi

say() { printf '\033[1m%s\033[0m %s\n' "→" "$*"; }

# BSD sed wants an argument to -i, GNU sed refuses one. Normalize.
sed_inplace() {
    if sed --version >/dev/null 2>&1; then
        sed -i "$@"
    else
        sed -i '' "$@"
    fi
}

# Latest version of an npm package, without needing npm installed.
# Takes the first "version" key so a greedy match can't drift onto a later one.
latest_version() {
    curl -fsSL "https://registry.npmjs.org/$1/latest" \
        | grep -o '"version"[[:space:]]*:[[:space:]]*"[^"]*"' \
        | head -1 \
        | sed 's/.*"\([^"]*\)"$/\1/'
}

# Rewrite a pinned VERSION line in this script so bumps show up in git diff.
pin() {
    local var="$1" version="$2"
    sed_inplace "s/^${var}=\".*\"/${var}=\"${version}\"/" "$SELF"
}

update_alpine() {
    local version="$ALPINE_VERSION"

    if [ "$USE_LATEST" -eq 1 ]; then
        version="$(latest_version alpinejs)"
        [ -n "$version" ] || { echo "could not resolve latest alpinejs" >&2; exit 1; }
    fi

    say "Alpine.js $version"
    mkdir -p "$LIB/alpine"
    curl -fsSL "https://cdn.jsdelivr.net/npm/alpinejs@${version}/dist/cdn.min.js" \
        -o "$LIB/alpine/alpine.min.js"

    [ "$version" = "$ALPINE_VERSION" ] || pin ALPINE_VERSION "$version"
}

update_fontawesome() {
    local version="$FONTAWESOME_VERSION"

    if [ "$USE_LATEST" -eq 1 ]; then
        version="$(latest_version @fortawesome/fontawesome-free)"
        [ -n "$version" ] || { echo "could not resolve latest fontawesome" >&2; exit 1; }
    fi

    say "FontAwesome Free $version"
    local tmp
    tmp="$(mktemp -d)"
    trap 'rm -rf "$tmp"' RETURN

    curl -fsSL "https://registry.npmjs.org/@fortawesome/fontawesome-free/-/fontawesome-free-${version}.tgz" \
        | tar -xz -C "$tmp"

    mkdir -p "$LIB/fontawesome/webfonts"
    cp "$tmp/package/css/all.min.css" "$LIB/fontawesome/fontawesome.min.css"

    # woff2 only — every browser we target supports it, and shipping the other
    # four formats triples the folder size for nobody's benefit.
    rm -f "$LIB/fontawesome/webfonts/"*.woff2
    cp "$tmp"/package/webfonts/*.woff2 "$LIB/fontawesome/webfonts/"

    # FontAwesome authors paths assuming css/ and webfonts/ are siblings.
    # We nest them together, so rewrite ../webfonts/ to webfonts/.
    sed_inplace 's|url(\.\./webfonts/|url(webfonts/|g' "$LIB/fontawesome/fontawesome.min.css"

    [ "$version" = "$FONTAWESOME_VERSION" ] || pin FONTAWESOME_VERSION "$version"
}

for target in "${TARGETS[@]}"; do
    "update_$target"
done

say "Done. Vendored files:"
find "$LIB" -type f \( -name '*.js' -o -name '*.css' \) -exec ls -lh {} \; \
    | awk '{ printf "    %-6s %s\n", $5, $9 }' \
    | sed "s|$ROOT/||"
