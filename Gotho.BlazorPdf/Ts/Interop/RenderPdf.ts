import * as pdfJsLib from "pdfjs-dist";
import {Pdf} from "../PdfDocument/Pdf";
import {EventBus, PDFLinkService, TextLayerBuilder} from 'pdfjs-dist/web/pdf_viewer.mjs';
import {RenderParameters} from "pdfjs-dist/types/src/display/api";
import {AnnotationLayerParameters} from "pdfjs-dist/types/src/display/annotation_layer";

const annotationIconMap: Record<string, string> = {
    "annotation-check.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-check.svg",
    "annotation-comment.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-comment.svg",
    "annotation-help.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-help.svg",
    "annotation-insert.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-insert.svg",
    "annotation-key.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-key.svg",
    "annotation-newparagraph.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-newparagraph.svg",
    "annotation-noicon.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-noicon.svg",
    "annotation-note.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-note.svg",
    "annotation-paperclip.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-paperclip.svg",
    "annotation-paragraph.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-paragraph.svg",
    "annotation-pushpin.svg": "_content/Gotho.BlazorPdf/pdfjs/annotation-pushpin.svg",
};

export async function queuePdfRender(pdf: Pdf, pageNumber: number | null) {
    if (pdf.renderInProgress) {
        if (pageNumber !== null) pdf.queuedPage = pageNumber;
        return;
    }
    await renderPdf(pdf);
}

export async function renderPdf(pdf: Pdf) {
    pdf.renderInProgress = true;

    try {
        if (pdf.scrollMode) {
            await renderScrollMode(pdf);
        } else {
            await renderSinglePage(pdf);
        }
    } finally {
        pdf.renderInProgress = false;

        // Handle queued requests
        if (pdf.queuedPage !== null) {
            pdf.queuedPage = null;
            await renderPdf(pdf);
        }
    }
}

async function renderSinglePage(pdf: Pdf) {
    const page = await pdf.document!.getPage(pdf.currentPage);
    const viewport = page.getViewport({scale: pdf.scale, rotation: pdf.rotation});

    // 1. Render Canvas
    pdf.canvas.width = viewport.width;
    pdf.canvas.height = viewport.height;
    await page.render({canvasContext: pdf.getCanvasContext(), viewport}).promise;

    // 2. Render Text Layer & Highlights
    const textLayerDiv = document.getElementById(`${pdf.id}_text`) as HTMLDivElement;
    setupLayerDiv(textLayerDiv, pdf.canvas);
    await renderTextLayer(page, viewport, textLayerDiv, pdf, pdf.getCanvasContext());

    // 3. Render Form Elements (Annotation Layer)
    const annotationDiv = document.getElementById(`${pdf.id}_annotations`) as HTMLDivElement;
    if (annotationDiv) {
        setupLayerDiv(annotationDiv, pdf.canvas);
        annotationDiv.replaceChildren();

        const annotationLayer = new pdfJsLib.AnnotationLayer({
            div: annotationDiv,
            accessibilityManager: null,
            annotationCanvasMap: new Map(),
            annotationEditorUIManager: null,
            page: page,
            viewport: viewport,
            structTreeLayer: null
        });

        const eventBus = new EventBus();
        const linkService = new PDFLinkService({eventBus});
        
        const annotationRenderParams: AnnotationLayerParameters = {
            viewport: viewport.clone({dontFlip: true}),
            div: annotationDiv,
            annotations: await page.getAnnotations(),
            page: page,
            linkService: linkService,
            renderForms: true
        }

        await annotationLayer.render(annotationRenderParams);

        // pdf-js uses images for certain annotation icons, we need to replace the src
        // so that it includes the correct path.
        const images = annotationDiv.querySelectorAll<HTMLImageElement>("img[src]");

        images.forEach(img => {
            const src = img.getAttribute("src");
            if (!src) return;

            const fileName = src.split("/").pop();
            if (!fileName) return;

            const replacement = annotationIconMap[fileName];
            if (!replacement) return;

            img.src = replacement;
        });
    }

    pdf.drawLayer.updateCanvas(pdf.currentPage, viewport.height, viewport.width, pdf.canvas.offsetLeft, pdf.canvas.offsetTop, pdf.rotation);
}

async function renderScrollMode(pdf: Pdf) {
    const container = document.getElementById(pdf.id) as HTMLElement;
    container.innerHTML = '';

    for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
        const page = await pdf.document!.getPage(pageNum);
        const viewport = page.getViewport({scale: pdf.scale, rotation: pdf.rotation});

        const canvas = document.createElement('canvas');
        canvas.id = `${pdf.id}-page-${pageNum}`;
        canvas.classList.add('blazorpdf__scroll-page');
        canvas.width = viewport.width;
        canvas.height = viewport.height;
        container.appendChild(canvas);

        const ctx = canvas.getContext('2d')!;
        await page.render({canvasContext: ctx, viewport}).promise;

        // Render Text Layer for scroll mode
        const textDiv = document.createElement('div');
        textDiv.classList.add('textLayer');
        const topGap = (pageNum - 1) * 16;
        textDiv.style.top = `${(viewport.height * (pageNum - 1)) + topGap}px`;
        textDiv.style.height = `${viewport.height}px`;
        textDiv.style.width = `${viewport.width}px`;
        container.appendChild(textDiv);

        await renderTextLayer(page, viewport, textDiv, pdf, ctx);
    }
}

function setupLayerDiv(div: HTMLDivElement, canvas: HTMLCanvasElement) {
    div.replaceChildren();
    div.style.left = canvas.offsetLeft + 'px';
    div.style.top = canvas.offsetTop + 'px';
    div.style.height = canvas.offsetHeight + 'px';
    div.style.width = canvas.offsetWidth + 'px';
}

async function renderTextLayer(page: pdfJsLib.PDFPageProxy, viewport: any, div: HTMLDivElement, pdf: Pdf, ctx: any) {
    const builder = new TextLayerBuilder({pdfPage: page});
    builder.div = div;

    const renderParams: RenderParameters = {
        canvasContext: ctx,
        viewport: viewport,
    }

    await builder.render(renderParams);

    if (pdf.previousQuery) {
        applySearchHighlights(div, pdf.previousQuery, pdf.activeSearchIndex);
    }
}

function applySearchHighlights(container: HTMLElement, query: string, activeIndex: number | null) {
    const spans = container.querySelectorAll('span');
    const lowerQuery = query.toLowerCase();
    let matchCount = -1;

    spans.forEach(span => {
        const text = span.textContent || "";
        const idx = text.toLowerCase().indexOf(lowerQuery);
        if (idx === -1) return;

        matchCount++;
        const before = text.slice(0, idx);
        const match = text.slice(idx, idx + query.length);
        const after = text.slice(idx + query.length);

        const className = (matchCount === activeIndex) ? "active" : "";
        span.innerHTML = `${before}<mark class="${className}">${match}</mark>${after}`;
    });
}