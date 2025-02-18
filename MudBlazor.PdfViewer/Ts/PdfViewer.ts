import {Pdf} from "./Pdf";

import {getDocument, GlobalWorkerOptions} from "pdfjs-dist"
GlobalWorkerOptions.workerSrc = "./pdfjs-4.0.379.worker.min.js";

export function init(dotnetReference: any, id: string, documentUrl: string, scale: number, rotation: number, singlePageMode: boolean) {
    console.log("Initializing PDF " + id);

    if (documentUrl) {
        const pdf = new Pdf(id, scale, rotation, documentUrl, singlePageMode)
        getDocument(pdf.url).promise.then((doc) => {
            pdf.setDocument(doc);
            renderPdf(pdf);
            renderThumbnails(dotnetReference, pdf);

            dotnetReference.invokeMethodAsync('DocumentLoaded', { pagesCount: pdf.pageCount, pageNumber: pdf.currentPage });
        })
    }
}

export function firstPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.firstPage()) {
        queuePdfRender(pdf, null);
        updateMetadata(dotnetReference, pdf);
    }
}

export function lastPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.lastPage()) {
        queuePdfRender(pdf, null);
        updateMetadata(dotnetReference, pdf);
    }
}

export function previousPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.previousPage()) {
        queuePdfRender(pdf, null);
        updateMetadata(dotnetReference, pdf);
    }
}

export function nextPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.nextPage()) {
        queuePdfRender(pdf, null);
        updateMetadata(dotnetReference, pdf);
    }
}

export function zoom(dotnetReference: any, id: string, scale: number) {
    const pdf = Pdf.getPdf(id)
    pdf.zoom(scale);
    queuePdfRender(pdf, null);
}

export function rotate(dotnetReference: any, id: string, rotation: number) {
    const pdf = Pdf.getPdf(id)
    pdf.rotate(rotation);
    queuePdfRender(pdf, null);
}

export function goToPage(dotnetReference: any, id: string, pageNumber: number) {
    const pdf = Pdf.getPdf(id);
    if (pdf !== null && pdf.gotoPage(pageNumber)) {

        if (pdf.singlePageMode) {
            // Change page
            queuePdfRender(pdf, null);
            updateMetadata(dotnetReference, pdf);
        } else {
            // Scroll to page
            const container = document.getElementById(id);
            const targetPage = document.getElementById(`${id}-page-${pageNumber}`);
            if (container && targetPage) {
                container.scrollTo({
                    top: targetPage.offsetTop - container.offsetTop,
                    behavior: 'smooth'
                });
                updateMetadata(dotnetReference, pdf);
            }
        }
    }
}

function queuePdfRender(pdf: Pdf, pageNumber: number) {
    if (pdf.renderInProgress) {
        if (!pageNumber) {
            pdf.queuedPage = pageNumber;
        }
        return;
    }

    renderPdf(pdf);
}

function renderPdf(pdf: Pdf) {
    pdf.renderInProgress = true;

    if (pdf.singlePageMode) {
        pdf.document.getPage(pdf.currentPage).then((pdfPage) => {
            const viewport = pdfPage.getViewport({scale: pdf.scale, rotation: pdf.rotation});
            pdf.canvas.width = viewport.width;
            pdf.canvas.height = viewport.height;

            const renderData = {
                canvasContext: pdf.getCanvasContext(),
                viewport: viewport
            }

            const renderTask = pdfPage.render(renderData);

            // Wait for rendering to finish
            renderTask.promise.then(() => {
                pdf.renderInProgress = false;
                if (pdf.queuedPage !== null) {
                    renderPdf(pdf);
                    pdf.queuedPage = null;
                }
            })
        })
    }
    else
    {
        const container = document.getElementById(pdf.id);
        const scale = pdf.scale;
        container.innerHTML = '';
        
        
        // @ts-ignore
        getDocument(pdf.url).promise.then(async function (doc) {
            for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
                const page = await doc.getPage(pageNum);
                const viewport = page.getViewport({ scale });

                // ✅ Create a canvas for each page
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d'); // ✅ Ensure canvas context is valid

                canvas.id = `${pdf.id}-page-${pageNum}`;
                canvas.classList.add('mudpdf_scroll_page');
                canvas.width = viewport.width;
                canvas.height = viewport.height;
                container.appendChild(canvas); // ✅ Append the canvas before rendering

                // ✅ Render the PDF page into the correct canvas
                await page.render({ canvasContext: ctx, viewport }).promise;
            }
        })
    }
}

// @ts-ignore
async function renderThumbnails(dotnetReference: any, pdf: Pdf) {
    const sidebar = document.getElementById(`${pdf.id}_thumbs`);
    sidebar.innerHTML = '';

    for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
        const page = await pdf.document.getPage(pageNum);

        let viewport = page.getViewport({scale: 0.2});
        let thumbCanvas = document.createElement('canvas');
        let thumbCtx = thumbCanvas.getContext('2d');

        thumbCanvas.width = viewport.width;
        thumbCanvas.height = viewport.height;
        thumbCanvas.classList.add('mudpdf_thumbnail');

        sidebar.appendChild(thumbCanvas);

        await page.render({canvasContext: thumbCtx, viewport}).promise;

        thumbCanvas.addEventListener('click', () => {
            goToPage(dotnetReference, pdf.id, pageNum);
        });
    }
}

function updateMetadata(dotnetReference: any, pdf: Pdf) {
    if (dotnetReference == null)
        return;

    dotnetReference.invokeMethodAsync('SetPdfViewerMetaData', {pagesCount: pdf.pageCount, pageNumber: pdf.currentPage});
}