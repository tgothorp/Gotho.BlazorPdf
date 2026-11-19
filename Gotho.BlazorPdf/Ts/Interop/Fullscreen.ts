const buttonSelector = "button[data-blazorpdf-fullscreen]";
const controllers = new Set<FullscreenController>();
const instances = new WeakMap<HTMLElement, FullscreenController>();
let removalObserver: MutationObserver | null = null;

// Delegate in capture phase: the viewer may be rerendered by Blazor or embedded
// in a clickable material row. Handle this button before ancestor handlers.
document.addEventListener("click", event => {
    const button = event.target instanceof Element ? event.target.closest(buttonSelector) : null;
    const root = button?.closest<HTMLElement>(".blazorpdf-container");
    if (!root) return;
    initializeFullscreen(root);
    instances.get(root)?.handleClick(event);
}, true);

interface Position {
    scroller: HTMLElement;
    page: HTMLElement | null;
    x: number;
    y: number;
}

/** Bind to the viewer, not a PDF ID: replacing a document must not exit fullscreen. */
export function initializeFullscreen(root: HTMLElement | null) {
    if (!root || instances.has(root)) return;
    const controller = new FullscreenController(root);
    instances.set(root, controller);
    controllers.add(controller);
    if (!removalObserver) {
        removalObserver = new MutationObserver(() => {
            for (const instance of controllers) {
                if (!instance.root.isConnected) instance.dispose();
            }
        });
        removalObserver.observe(document.documentElement, {childList: true, subtree: true});
    }
}

class FullscreenController {
    private fallback = false;
    private pending = false;
    private wantsFullscreen = false;
    private requestToken = 0;
    private requestTimer = 0;
    private wasNative = false;
    private disposed = false;
    private frame = 0;
    private position: Position | null = null;
    private documentPosition: {x: number; y: number} | null = null;
    private ancestors: HTMLElement[] = [];
    private previousFocus: HTMLElement | null = null;

    constructor(readonly root: HTMLElement) {
        document.addEventListener("fullscreenchange", this.onFullscreenChange);
        document.addEventListener("keydown", this.onKeyDown, true);
    }

    private get active() {
        return this.fallback || document.fullscreenElement === this.root;
    }

    handleClick = (event: MouseEvent) => {
        const target = event.target instanceof Element ? event.target.closest(buttonSelector) : null;
        if (!target || target.closest(".blazorpdf-container") !== this.root) return;
        event.preventDefault();
        event.stopPropagation();
        if (this.pending || this.active) {
            this.exit();
            return;
        }
        this.position = this.capture();
        this.documentPosition ??= {x: window.scrollX, y: window.scrollY};
        this.wantsFullscreen = true;
        const token = ++this.requestToken;
        this.previousFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null;
        const other = [...controllers].find(controller => controller !== this && controller.active);
        if (other) {
            other.exit();
            this.enterFallback();
            return;
        }
        if (!this.root.requestFullscreen || document.fullscreenEnabled === false) {
            this.enterFallback();
            return;
        }
        // No .NET round trip or await before this call: retain transient user activation.
        this.pending = true;
        // Some embedded browsers expose the API but never settle its promise.
        // Keep an exit path and provide the same usable full-window fallback.
        this.requestTimer = window.setTimeout(() => {
            if (!this.disposed && token === this.requestToken && this.wantsFullscreen) {
                this.pending = false;
                this.enterFallback();
            }
        }, 2000);
        try {
            this.root.requestFullscreen({navigationUI: "hide"}).then(() => {
                if (this.disposed || token !== this.requestToken || !this.wantsFullscreen) {
                    if (document.fullscreenElement === this.root)
                        void document.exitFullscreen().catch(() => {});
                    return;
                }
                clearTimeout(this.requestTimer);
                this.pending = false;
                this.fallback = false;
                this.clearAncestors();
                this.sync();
                this.focusButton();
            }, () => {
                if (this.disposed || token !== this.requestToken || !this.wantsFullscreen) return;
                clearTimeout(this.requestTimer);
                this.pending = false;
                this.enterFallback();
            });
        } catch {
            clearTimeout(this.requestTimer);
            this.pending = false;
            this.enterFallback();
        }
    };

    private enterFallback() {
        if (this.disposed || !this.wantsFullscreen || this.fallback) return;
        this.fallback = true;
        // Do not move Blazor-owned DOM into body. Neutralize clipping/containing
        // ancestors temporarily so fixed positioning also works inside modals.
        for (let element = this.root.parentElement; element; element = element.parentElement) {
            if (!element.classList.contains("blazorpdf__fullscreen-ancestor")) {
                element.classList.add("blazorpdf__fullscreen-ancestor");
                this.ancestors.push(element);
            }
        }
        this.sync();
        this.focusButton();
    }

    private exit() {
        this.position = this.capture();
        this.wantsFullscreen = false;
        this.requestToken++;
        clearTimeout(this.requestTimer);
        this.pending = false;
        if (document.fullscreenElement === this.root) {
            void document.exitFullscreen().catch(() => {});
        } else {
            this.fallback = false;
            this.clearAncestors();
            this.sync();
            this.restoreFocus();
        }
    }

    private onFullscreenChange = () => {
        if (this.disposed) return;
        const native = document.fullscreenElement === this.root;
        if (!native && !this.wasNative) return;
        this.wasNative = native;
        if (native && !this.wantsFullscreen) {
            void document.exitFullscreen().catch(() => {});
            return;
        }
        if (!this.position) this.position = this.capture();
        clearTimeout(this.requestTimer);
        this.pending = false;
        this.fallback = false;
        this.clearAncestors();
        if (!native) {
            this.wantsFullscreen = false;
            this.requestToken++;
        }
        this.sync();
        if (!this.active) this.restoreFocus();
    };

    private sync() {
        this.root.classList.toggle("blazorpdf--fullscreen", this.active);
        this.root.classList.toggle("blazorpdf--fullscreen-fallback", this.fallback);
        for (const button of Array.from(this.root.querySelectorAll<HTMLButtonElement>(buttonSelector))) {
            const label = this.active ? button.dataset.fullscreenExit : button.dataset.fullscreenEnter;
            button.setAttribute("aria-label", label || (this.active ? "Exit full screen" : "Full screen"));
            button.title = button.getAttribute("aria-label")!;
            button.setAttribute("aria-pressed", String(this.active));
        }
        document.documentElement.classList.toggle("blazorpdf__fullscreen-document",
            [...controllers].some(controller => controller.active));
        cancelAnimationFrame(this.frame);
        // Allow layout and ScrollViewer's ResizeObserver to settle before restoring
        // the page anchor in the new viewport.
        this.frame = requestAnimationFrame(() => {
            this.frame = requestAnimationFrame(() => {
                this.frame = 0;
                this.restorePosition();
                if (!this.active && this.documentPosition) {
                    window.scrollTo(this.documentPosition.x, this.documentPosition.y);
                    this.documentPosition = null;
                }
            });
        });
    }

    private measure(scroller: HTMLElement, page: HTMLElement) {
        const outer = scroller.getBoundingClientRect();
        const inner = page.getBoundingClientRect();
        const scaleX = outer.width / scroller.offsetWidth || 1;
        const scaleY = outer.height / scroller.offsetHeight || 1;
        return {
            left: (inner.left - outer.left) / scaleX + scroller.scrollLeft - scroller.clientLeft,
            top: (inner.top - outer.top) / scaleY + scroller.scrollTop - scroller.clientTop,
            width: inner.width / scaleX,
            height: inner.height / scaleY,
        };
    }

    private capture(): Position | null {
        const scrollMode = this.root.querySelector<HTMLElement>(".blazorpdf__scroll");
        const scroller = scrollMode || this.root.querySelector<HTMLElement>(".blazorpdf-pdf");
        if (!scroller) return null;
        const pages = scrollMode
            ? Array.from(scrollMode.querySelectorAll<HTMLElement>(".blazorpdf__page"))
            : Array.from(this.root.querySelectorAll<HTMLElement>(".blazorpdf__page-frame canvas:not(.blazorpdf-drawing__canvas)"));
        for (const page of pages) {
            const box = this.measure(scroller, page);
            if (box.height && box.width && box.top + box.height > scroller.scrollTop) {
                return {scroller, page,
                    x: (scroller.scrollLeft + scroller.clientWidth / 2 - box.left) / box.width,
                    y: (scroller.scrollTop - box.top) / box.height};
            }
        }
        return {scroller, page: null, x: scroller.scrollLeft, y: scroller.scrollTop};
    }

    private restorePosition() {
        const position = this.position;
        this.position = null;
        if (!position || !position.scroller.isConnected || this.disposed) return;
        if (position.page?.isConnected && this.root.contains(position.page)) {
            const box = this.measure(position.scroller, position.page);
            position.scroller.scrollLeft = box.left + position.x * box.width - position.scroller.clientWidth / 2;
            position.scroller.scrollTop = box.top + position.y * box.height;
        } else if (!position.page) {
            position.scroller.scrollLeft = position.x;
            position.scroller.scrollTop = position.y;
        }
    }

    private onKeyDown = (event: KeyboardEvent) => {
        if (!this.active) return;
        if (event.key === "Escape") {
            event.preventDefault();
            event.stopPropagation();
            this.exit();
        } else if (event.key === "Tab" && this.fallback) {
            const focusable = Array.from(this.root.querySelectorAll<HTMLElement>(
                "button:not([disabled]), a[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex='-1'])"))
                .filter(element => element.tabIndex >= 0 && element.getClientRects().length &&
                    getComputedStyle(element).visibility !== "hidden");
            const first = focusable[0], last = focusable[focusable.length - 1];
            if (!first) return;
            if (!this.root.contains(document.activeElement) ||
                (event.shiftKey && document.activeElement === first) ||
                (!event.shiftKey && document.activeElement === last)) {
                event.preventDefault();
                (event.shiftKey ? last : first).focus({preventScroll: true});
            }
        }
    };

    private focusButton() {
        this.root.querySelector<HTMLButtonElement>(buttonSelector)?.focus({preventScroll: true});
    }

    private restoreFocus() {
        if (this.previousFocus?.isConnected) this.previousFocus.focus({preventScroll: true});
        this.previousFocus = null;
    }

    private clearAncestors() {
        for (const element of this.ancestors) element.classList.remove("blazorpdf__fullscreen-ancestor");
        this.ancestors = [];
    }

    dispose() {
        this.disposed = true;
        this.wantsFullscreen = false;
        this.requestToken++;
        clearTimeout(this.requestTimer);
        cancelAnimationFrame(this.frame);
        if (document.fullscreenElement === this.root)
            void document.exitFullscreen().catch(() => {});
        this.fallback = false;
        this.clearAncestors();
        this.root.classList.remove("blazorpdf--fullscreen", "blazorpdf--fullscreen-fallback");
        document.removeEventListener("fullscreenchange", this.onFullscreenChange);
        document.removeEventListener("keydown", this.onKeyDown, true);
        controllers.delete(this);
        instances.delete(this.root);
        document.documentElement.classList.toggle("blazorpdf__fullscreen-document",
            [...controllers].some(controller => controller.active));
        if (!controllers.size) {
            removalObserver?.disconnect();
            removalObserver = null;
        }
    }
}
