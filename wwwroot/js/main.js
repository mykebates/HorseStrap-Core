/* =========================================================================
   main.js — your JavaScript entry point

   Loaded as a module (`<script type="module">`), so top-level `const` is
   scoped to this file, `await` works at the top level, and you can
   `import` other modules from wwwroot/js/ whenever you want to split
   things up.

   Alpine is loaded separately and deferred in _Layout.cshtml. This file
   runs BEFORE Alpine boots, which is exactly when you want to register
   components and stores — hence the `alpine:init` listener below.

   ---------------------------------------------------------------------
   ALPINE IN 60 SECONDS

   Most interactivity needs no JS file at all. Put it in the markup:

     x-data="{ open: false }"    declare reactive state on an element
     x-show="open"               toggle visibility (keeps it in the DOM)
     x-if="open"                 add/remove from the DOM (needs <template>)
     x-on:click="open = !open"   listen for events        (shorthand: @click)
     x-bind:class="{ 'is-open': open }"   bind an attribute (shorthand: :class)
     x-model="query"             two-way bind a form input
     x-for="item in items"       loop                     (needs <template>)
     x-text="count"              set textContent
     x-html="markup"             set innerHTML (only with content you trust)
     x-transition                animate enter/leave
     x-effect="console.log(q)"   re-run whenever a dependency changes
     x-ref="input" / $refs.input reference an element
     x-cloak                     hide until Alpine initializes

   A working dropdown, no JS file involved:

     <div x-data="{ open: false }" @click.outside="open = false">
         <button @click="open = !open" :aria-expanded="open">Menu</button>
         <ul x-show="open" x-transition x-cloak>
             <li><a href="/one">One</a></li>
         </ul>
     </div>

   Docs: https://alpinejs.dev/start-here
   ========================================================================= */

document.addEventListener("alpine:init", () => {
    /* ---------------------------------------------------------------------
       STORES — global state, readable from any component on the page.
       Access anywhere with $store.ui, or Alpine.store('ui') from JS.
       --------------------------------------------------------------------- */

    Alpine.store("ui", {
        navOpen: false,

        toggleNav() {
            this.navOpen = !this.navOpen;
            // Prevent the page behind an open mobile nav from scrolling.
            document.body.classList.toggle("no-scroll", this.navOpen);
        },

        closeNav() {
            this.navOpen = false;
            document.body.classList.remove("no-scroll");
        },
    });

    /* ---------------------------------------------------------------------
       COMPONENTS — reusable behaviour, applied with x-data="name()".

       Inline x-data is fine for small things. Reach for Alpine.data() when
       the logic gets long enough to clutter the markup, or when you want
       the same behaviour on several elements.
       --------------------------------------------------------------------- */

    /*
        <div x-data="disclosure">
            <button @click="toggle" :aria-expanded="open">Details</button>
            <div x-show="open">...</div>
        </div>
    */
    Alpine.data("disclosure", (initiallyOpen = false) => ({
        open: initiallyOpen,

        toggle() {
            this.open = !this.open;
        },

        close() {
            this.open = false;
        },
    }));

    /*
        <div x-data="filterList([{ name: 'Palomino' }, { name: 'Appaloosa' }])">
            <input type="search" x-model="query" placeholder="Search...">
            <ul>
                <template x-for="item in results" :key="item.name">
                    <li x-text="item.name"></li>
                </template>
            </ul>
            <p x-show="!results.length">No matches.</p>
        </div>
    */
    Alpine.data("filterList", (items = []) => ({
        items,
        query: "",

        // A getter recomputes automatically whenever `query` changes.
        get results() {
            const q = this.query.trim().toLowerCase();
            if (!q) return this.items;

            return this.items.filter((item) =>
                JSON.stringify(item).toLowerCase().includes(q)
            );
        },
    }));

    /*
        <button x-data="copyButton" @click="copy($refs.snippet.textContent)">
            <span x-text="copied ? 'Copied' : 'Copy'"></span>
        </button>
    */
    Alpine.data("copyButton", () => ({
        copied: false,

        async copy(text) {
            try {
                await navigator.clipboard.writeText(text);
                this.copied = true;
                setTimeout(() => (this.copied = false), 2000);
            } catch {
                this.copied = false;
            }
        },
    }));
});

/* =========================================================================
   Plain JS below this line — anything that isn't Alpine's business.
   ========================================================================= */

document.addEventListener("DOMContentLoaded", () => {
    if (document.body.dataset.env === "Development") {
        console.log("%c🐴 Horsin' around", "color:#a78bfa;font-weight:bold");
    }
});
