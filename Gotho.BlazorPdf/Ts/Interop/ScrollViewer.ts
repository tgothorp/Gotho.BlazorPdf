import {PDFPageProxy, RenderTask} from "pdfjs-dist";
import {Pdf} from "../PdfDocument/Pdf";
import {renderTextLayer} from "./RenderPdf";
import DotNetObject = DotNet.DotNetObject;

// CSS dimensions are independent of the backing-store resolution.
export function outputScale(width: number, height: number): number {
    return Math.min(window.devicePixelRatio || 1, 2,
        Math.sqrt(8_000_000 / (width * height)), 8192 / width, 8192 / height);
}

interface PageView {
    page: PDFPageProxy;
    element: HTMLDivElement;
    canvas: HTMLCanvasElement;
    text: HTMLDivElement;
    top: number;
    left: number;
    width: number;
    height: number;
    rendered: number;
}

interface Anchor {
    index: number;
    x: number;
    y: number;
    screenX: number;
    screenY: number;
}

interface Gesture {
    distance: number;
    scale: number;
    ratio: number;
    startX: number;
    startY: number;
    x: number;
    y: number;
    anchor: Anchor;
}

/** One controller per viewer: lazy canvases, bounded resolution and local touch feedback. */
export class ScrollViewer {
    private content = document.createElement("div");
    private pages: PageView[] = [];
    private version = 0;
    private scale = 0;
    private rotation = -1;
    private disposed = false;
    private rendering: Promise<void> | null = null;
    private pending = false;
    private task: RenderTask | null = null;
    private gesture: Gesture | null = null;
    private syncing = false;
    private resize: ResizeObserver;
    private frame = 0;
    private container: HTMLElement;

    constructor(private pdf: Pdf, private dotnet: DotNetObject) {
        this.container = document.getElementById(pdf.id)!;
        this.content.className = "blazorpdf__scroll-content";
        this.container.replaceChildren(this.content);
        this.container.addEventListener("scroll", this.onScroll, {passive: true});
        this.container.addEventListener("touchstart", this.onTouchStart, {passive: false});
        this.container.addEventListener("touchmove", this.onTouchMove, {passive: false});
        this.container.addEventListener("touchend", this.onTouchEnd, {passive: false});
        this.container.addEventListener("touchcancel", this.onTouchCancel, {passive: false});
        this.resize = new ResizeObserver(() => {
            if (this.gesture) this.cancelGesture();
            this.layout();
            this.schedule();
        });
        this.resize.observe(this.container);
    }

    async initialize() {
        for (let number = 1; number <= this.pdf.pageCount; number++) {
            const page = await this.pdf.document!.getPage(number);
            if (this.disposed) return;
            const element = document.createElement("div");
            element.id = `${this.pdf.id}-page-${number}`;
            element.className = "blazorpdf__page";
            const canvas = document.createElement("canvas");
            canvas.width = canvas.height = 0;
            const text = document.createElement("div");
            text.className = "textLayer";
            element.append(canvas, text);
            this.content.appendChild(element);
            this.pages.push({page, element, canvas, text, top: 0, left: 0,
                width: 0, height: 0, rendered: -1});
        }
    }

    async update() {
        if (this.disposed) return;
        if (this.scale !== this.pdf.scale || this.rotation !== this.pdf.rotation) {
            const anchor = this.pages.length && this.scale
                ? this.capture(this.container.clientWidth / 2, this.container.clientHeight / 2) : null;
            this.cancelGesture();
            this.version++;
            this.task?.cancel();
            this.scale = this.pdf.scale;
            this.rotation = this.pdf.rotation;
            this.layout();
            if (anchor) this.restore(anchor);
        }
        await this.refresh();
    }

    private layout() {
        if (this.disposed || !this.scale) return;
        let width = this.container.clientWidth;
        for (const view of this.pages) {
            const viewport = view.page.getViewport({scale: this.scale, rotation: this.rotation});
            view.width = viewport.width;
            view.height = viewport.height;
            width = Math.max(width, viewport.width);
        }
        let top = 0;
        for (const view of this.pages) {
            view.top = top;
            view.left = (width - view.width) / 2;
            Object.assign(view.element.style, {
                top: `${top}px`, left: `${view.left}px`,
                width: `${view.width}px`, height: `${view.height}px`,
            });
            view.element.style.setProperty("--total-scale-factor", `${this.scale}`);
            top += view.height + 16;
        }
        this.content.style.width = `${width}px`;
        this.content.style.height = `${top}px`;
    }

    private capture(x: number, y: number): Anchor {
        const contentY = this.container.scrollTop + y;
        let index = this.pages.findIndex(view => contentY < view.top + view.height + 16);
        if (index < 0) index = this.pages.length - 1;
        const view = this.pages[index];
        return {index, x: (this.container.scrollLeft + x - view.left) / view.width,
            y: (contentY - view.top) / view.height, screenX: x, screenY: y};
    }

    private restore(anchor: Anchor) {
        const view = this.pages[anchor.index];
        this.container.scrollLeft = view.left + anchor.x * view.width - anchor.screenX;
        this.container.scrollTop = view.top + anchor.y * view.height - anchor.screenY;
    }

    private onScroll = () => this.schedule();

    private schedule() {
        if (this.disposed || this.frame || this.gesture) return;
        this.frame = requestAnimationFrame(() => {
            this.frame = 0;
            void this.refresh().catch(error => console.error("PDF scroll rendering failed", error));
        });
    }

    private async refresh() {
        if (this.disposed || this.gesture) return;
        this.pending = true;
        if (this.rendering) return this.rendering;
        this.rendering = this.renderVisible();
        try {
            await this.rendering;
        } finally {
            this.rendering = null;
        }
    }

    private async renderVisible() {
        while (this.pending && !this.disposed) {
            this.pending = false;
            const version = this.version;
            const top = this.container.scrollTop;
            const height = this.container.clientHeight;
            const center = top + height / 2;
            const candidates = this.pages
                .filter(view => view.top + view.height >= top - height && view.top <= top + height * 2)
                .sort((a, b) => Math.abs(a.top + a.height / 2 - center) - Math.abs(b.top + b.height / 2 - center));

            // Evict distant canvases and stale layers: total memory does not grow with page count.
            for (const view of this.pages) {
                if (!candidates.includes(view)) this.release(view);
            }
            let pixels = 0;
            for (const view of candidates) {
                if (this.disposed || version !== this.version || this.gesture) break;
                const visible = view.top + view.height >= top && view.top <= top + height;
                const factor = outputScale(view.width, view.height);
                const cost = Math.ceil(view.width * factor) * Math.ceil(view.height * factor);
                if (!visible && pixels + cost > 24_000_000) {
                    this.release(view);
                    continue;
                }
                pixels += cost;
                if (view.rendered === version) continue;
                const viewport = view.page.getViewport({scale: this.scale, rotation: this.rotation});
                view.text.replaceChildren();
                view.canvas.width = Math.ceil(view.width * factor);
                view.canvas.height = Math.ceil(view.height * factor);
                view.canvas.style.width = `${view.width}px`;
                view.canvas.style.height = `${view.height}px`;
                try {
                    this.task = view.page.render({
                        canvasContext: view.canvas.getContext("2d")!, viewport,
                        transform: [factor, 0, 0, factor, 0, 0],
                    });
                    await this.task.promise;
                    if (this.disposed || version !== this.version) break;
                    await renderTextLayer(view.page, viewport, view.text, this.pdf, view.canvas.getContext("2d"));
                    if (this.disposed || version !== this.version) {
                        view.text.replaceChildren();
                        break;
                    }
                    view.rendered = version;
                } catch (error: any) {
                    if (error?.name !== "RenderingCancelledException") throw error;
                } finally {
                    this.task = null;
                }
            }
        }
    }

    private release(view: PageView) {
        if (view.canvas.width) view.canvas.width = view.canvas.height = 0;
        view.text.replaceChildren();
        view.rendered = -1;
    }

    private midpoint(event: TouchEvent) {
        const rect = this.container.getBoundingClientRect();
        const a = event.touches[0], b = event.touches[1];
        return {x: (a.clientX + b.clientX) / 2 - rect.left - this.container.clientLeft,
            y: (a.clientY + b.clientY) / 2 - rect.top - this.container.clientTop,
            distance: Math.hypot(a.clientX - b.clientX, a.clientY - b.clientY)};
    }

    private onTouchStart = (event: TouchEvent) => {
        if (event.touches.length !== 2 || !this.pages.length || this.syncing) return;
        event.preventDefault();
        if (this.gesture) return;
        const point = this.midpoint(event);
        if (point.distance < 1) return;
        this.gesture = {distance: point.distance, scale: this.scale, ratio: 1,
            startX: point.x, startY: point.y, x: point.x, y: point.y,
            anchor: this.capture(point.x, point.y)};
        this.content.style.transformOrigin =
            `${this.container.scrollLeft + point.x}px ${this.container.scrollTop + point.y}px`;
        this.content.style.willChange = "transform";
    };

    private onTouchMove = (event: TouchEvent) => {
        if (!this.gesture || event.touches.length !== 2) return;
        event.preventDefault();
        const point = this.midpoint(event);
        const gesture = this.gesture;
        gesture.ratio = Math.max(0.25, Math.min(5, gesture.scale * point.distance / gesture.distance)) / gesture.scale;
        gesture.x = point.x;
        gesture.y = point.y;
        this.content.style.transform =
            `translate(${point.x - gesture.startX}px, ${point.y - gesture.startY}px) scale(${gesture.ratio})`;
    };

    private cancelGesture() {
        this.gesture = null;
        this.content.style.removeProperty("transform");
        this.content.style.removeProperty("transform-origin");
        this.content.style.removeProperty("will-change");
    }

    private onTouchCancel = () => {
        this.cancelGesture();
        this.schedule();
    };

    private onTouchEnd = (event: TouchEvent) => {
        if (!this.gesture || event.touches.length >= 2) return;
        event.preventDefault();
        const gesture = this.gesture;
        this.cancelGesture();
        this.pdf.scale = this.scale = gesture.scale * gesture.ratio;
        this.version++;
        this.task?.cancel();
        this.layout();
        this.restore({...gesture.anchor, screenX: gesture.x, screenY: gesture.y});
        this.syncing = true;
        void this.dotnet.invokeMethodAsync("SetTouchZoom", this.pdf.id, this.scale)
            .catch(error => console.error("PDF touch zoom synchronization failed", error))
            .then(() => { this.syncing = false; });
        this.schedule();
    };

    dispose() {
        this.disposed = true;
        this.task?.cancel();
        this.resize.disconnect();
        cancelAnimationFrame(this.frame);
        this.cancelGesture();
        this.container.removeEventListener("scroll", this.onScroll);
        this.container.removeEventListener("touchstart", this.onTouchStart);
        this.container.removeEventListener("touchmove", this.onTouchMove);
        this.container.removeEventListener("touchend", this.onTouchEnd);
        this.container.removeEventListener("touchcancel", this.onTouchCancel);
        for (const view of this.pages) {
            // A cancelled render may still be unwinding; release only after it settles.
            void (this.rendering ?? Promise.resolve()).catch(() => {}).then(() => this.release(view));
        }
    }
}
