import {PdfState} from "../PdfDocument/PdfState";
import {Pdf} from "../PdfDocument/Pdf";
import {GlobalWorkerOptions, getDocument} from "pdfjs-dist";
import {renderThumbnails} from "./RenderThumbnails"
import {renderPdf} from "./RenderPdf";

import DotNetObject = DotNet.DotNetObject;

GlobalWorkerOptions.workerSrc = "./pdf.worker-5.1.91.min.mjs";
let workerInitialised = false;

/**
 * This is a work-around for .NET MAUI, the MAUI browser used by Blazor cannot load
 * the required worker directly, so we must grab it via a fetch().
 */
async function setupProjectWorker() {
    const response = await fetch('./pdf.worker-5.1.91.min.mjs');
    const workerCode = await response.text();

    const blob = new Blob([workerCode], {type: 'application/javascript'});
    GlobalWorkerOptions.workerSrc = URL.createObjectURL(blob);
    workerInitialised = true;
}

export async function initPdfViewer(dotnetReference: DotNetObject, pdfDto: PdfState, useProjectWorker: boolean): Promise<void> {
    console.log("Initializing PDF " + pdfDto.id);

    if (useProjectWorker && !workerInitialised) {
        await setupProjectWorker();
    } else {
        workerInitialised = true;
    }

    try {
        const pdf = new Pdf(pdfDto)
        const loadedDocument = await getDocument(pdf.getDocumentInitParams()).promise;
        await pdf.setDocument(loadedDocument)
        await renderPdf(pdf)
        await renderThumbnails(dotnetReference, pdf)

        await dotnetReference.invokeMethodAsync('DocumentLoaded', {
            currentPage: pdf.currentPage,
            totalPages: pdf.pageCount
        });
    } catch (err: any) {
        await dotnetReference.invokeMethodAsync('PdfViewerError', {name: err.name, message: err.message});
    }
}
