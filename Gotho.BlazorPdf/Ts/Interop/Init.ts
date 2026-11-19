import {PdfState} from "../PdfDocument/PdfState";
import {Pdf} from "../PdfDocument/Pdf";
import {GlobalWorkerOptions, getDocument} from "pdfjs-dist";
import {renderThumbnails} from "./RenderThumbnails"
import {renderPdf} from "./RenderPdf";
import {ScrollViewer} from "./ScrollViewer";
import {initializeFullscreen} from "./Fullscreen";

import DotNetObject = DotNet.DotNetObject;

// Worker() resolves relative strings against the document, while import()
// resolves them against this module. Always use one absolute asset URL so a
// missing root-level file cannot silently force PDF.js onto the main thread.
const bundledWorkerUrl = new URL("./pdf.worker-5.1.91.min.mjs", import.meta.url).href;
GlobalWorkerOptions.workerSrc = bundledWorkerUrl;
let workerSetup: Promise<void> | undefined;
interface Initialization {
    cancelled: boolean;
    promise: Promise<void>;
    pdf?: Pdf;
}
const initializations = new Map<string, Initialization>();

/**
 * This is a work-around for .NET MAUI, the MAUI browser used by Blazor cannot load
 * the required worker directly, so we must grab it via a fetch().
 */
async function setupProjectWorker() {
    let response = await fetch(new URL("pdf.worker-5.1.91.min.mjs", document.baseURI).href);
    if (!response.ok || response.headers.get("Content-Type")?.includes("text/html"))
        response = await fetch(bundledWorkerUrl);
    if (!response.ok)
        throw new Error(`Cannot load PDF.js worker (${response.status}): ${response.url}`);
    const workerCode = await response.text();

    const blob = new Blob([workerCode], {type: 'application/javascript'});
    GlobalWorkerOptions.workerSrc = URL.createObjectURL(blob);
}

export function initPdfViewer(dotnetReference: DotNetObject, pdfDto: PdfState, useProjectWorker: boolean): Promise<void> {
    const existing = initializations.get(pdfDto.id);
    if (existing) return existing.promise;
    const operation: Initialization = {cancelled: false, promise: Promise.resolve()};
    initializations.set(pdfDto.id, operation);
    operation.promise = initializePdf(dotnetReference, pdfDto, useProjectWorker, operation);
    return operation.promise;
}

async function initializePdf(dotnetReference: DotNetObject, pdfDto: PdfState,
                             useProjectWorker: boolean, operation: Initialization): Promise<void> {
    console.log("Initializing PDF " + pdfDto.id);

    try {
        initializeFullscreen(document.getElementById(pdfDto.id)?.closest<HTMLElement>(".blazorpdf-container") || null);
        if (useProjectWorker) {
            try {
                await (workerSetup ??= setupProjectWorker());
            } catch (error) {
                workerSetup = undefined;
                throw error;
            }
        }
        if (operation.cancelled) return;
        const pdf = operation.pdf = new Pdf(pdfDto);
        pdf.loadingTask = getDocument(pdf.getDocumentInitParams());
        const loadedDocument = await pdf.loadingTask.promise;
        if (operation.cancelled || pdf.disposed) return;
        await pdf.setDocument(loadedDocument);
        if (operation.cancelled || pdf.disposed) return;
        if (pdf.scrollMode) {
            pdf.scrollViewer = new ScrollViewer(pdf, dotnetReference);
            await pdf.scrollViewer.initialize();
        }
        if (operation.cancelled || pdf.disposed) return;
        await renderPdf(pdf);
        if (operation.cancelled || pdf.disposed) return;
        await renderThumbnails(dotnetReference, pdf);
        if (operation.cancelled || pdf.disposed) return;

        await dotnetReference.invokeMethodAsync('DocumentLoaded', {
            currentPage: pdf.currentPage,
            totalPages: pdf.pageCount
        });
    } catch (err: any) {
        if (operation.cancelled || operation.pdf?.disposed) return;
        console.error("PDF initialization failed", err);
        await dotnetReference.invokeMethodAsync('PdfViewerError', {name: err.name, message: err.message});
    } finally {
        if (initializations.get(pdfDto.id) === operation)
            initializations.delete(pdfDto.id);
    }
}

export async function disposePdfViewer(id: string) {
    const operation = initializations.get(id);
    if (operation) operation.cancelled = true;
    initializations.delete(id);
    await Pdf.dispose(id);
}
