import {Pdf} from "./Pdf";
import {PdfState} from "./PdfState";
import {saveAs} from "file-saver"
import * as pdfjs from "pdfjs-dist"
import { TextLayerBuilder } from 'pdfjs-dist/web/pdf_viewer.mjs';

// @ts-ignore
import printjs from "print-js"

pdfjs.GlobalWorkerOptions.workerSrc = "./pdf.worker.min.mjs";

export function initPdfViewer(dotnetReference: any, pdfDto: PdfState, singlePageMode: boolean) {
    console.log("Initializing PDF " + pdfDto.id);

    if (pdfDto.url) {
        const pdf = new Pdf(pdfDto.id, pdfDto.scale, pdfDto.orientation, pdfDto.url, singlePageMode, null)
        pdfjs.getDocument({url: pdf.url}).promise.then(doc => {
            pdf.setDocument(doc)
            renderPdf(pdf)

            dotnetReference.invokeMethodAsync('DocumentLoaded', {
                currentPage: pdf.currentPage,
                totalPages: pdf.pageCount
            });
        })
    }
}

export function updatePdf(dotnetReference: any, pdfDto: PdfState) {
    console.log("Updating PDF " + pdfDto.id);
    const pdf = Pdf.getPdf(pdfDto.id)
    pdf.updatePdf(pdfDto)
    queuePdfRender(pdf, null);
    updateMetadata(dotnetReference, pdf)
}

export function init(dotnetReference: any, id: string, documentUrl: string, scale: number, rotation: number, singlePageMode: boolean, password: string = null) {
    console.log("Initializing PDF " + id);

    if (documentUrl) {
        const pdf = new Pdf(id, scale, rotation, documentUrl, singlePageMode, password)
        const documentInit = pdf.password
            ? {url: pdf.url, password: pdf.password}
            : {url: pdf.url};

        pdfjs.getDocument(documentInit).promise.then((doc) => {
            pdf.setDocument(doc);
            renderPdf(pdf);
            renderThumbnails(dotnetReference, pdf);

            dotnetReference.invokeMethodAsync('DocumentLoaded', {
                pagesCount: pdf.pageCount,
                pageNumber: pdf.currentPage
            });
        }).catch((err) => {
            dotnetReference.invokeMethodAsync('PdfViewerError', {name: err.name, message: err.message});
        })
    }
}

export function firstPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.firstPage()) {
        if (pdf.singlePageMode) {
            queuePdfRender(pdf, null);
        } else {
            scrollToPage(id, pdf.currentPage);
        }
        updateMetadata(dotnetReference, pdf);
    }
}

export function lastPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.lastPage()) {
        if (pdf.singlePageMode) {
            queuePdfRender(pdf, null);
        } else {
            scrollToPage(id, pdf.currentPage);
        }

        updateMetadata(dotnetReference, pdf);
    }
}

export function previousPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.previousPage()) {
        if (pdf.singlePageMode) {
            queuePdfRender(pdf, null);
        } else {
            scrollToPage(id, pdf.currentPage);
        }
        updateMetadata(dotnetReference, pdf);
    }
}

export function nextPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id)
    if (pdf !== null && pdf.nextPage()) {
        if (pdf.singlePageMode) {
            queuePdfRender(pdf, null);
        } else {
            scrollToPage(id, pdf.currentPage);
        }
        updateMetadata(dotnetReference, pdf);
    }
}

export function zoom(dotnetReference: any, id: string, scale: number) {
    const pdf = Pdf.getPdf(id)
    pdf.zoom(scale);
    queuePdfRender(pdf, null);
    document.body.style.setProperty('--scale-factor', `${scale}`);
    if (pdf.singlePageMode) {
        scrollToPage(id, pdf.currentPage);
    }
}

export function rotate(dotnetReference: any, id: string, rotation: number) {
    const pdf = Pdf.getPdf(id)
    pdf.rotate(rotation);
    queuePdfRender(pdf, null);
    if (pdf.singlePageMode) {
        scrollToPage(id, pdf.currentPage);
    }
}

export function goToPage(dotnetReference: any, id: string, pageNumber: number) {
    const pdf = Pdf.getPdf(id);
    if (pdf.gotoPage(pageNumber)) {
        if (pdf.singlePageMode) {
            // Change page
            queuePdfRender(pdf, null);
            updateMetadata(dotnetReference, pdf);
        } else {
            // Scroll to page
            scrollToPage(id, pageNumber);
            updateMetadata(dotnetReference, pdf);
        }
    }
}

export function printDocument(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    if (!pdf.password) {
        if (pdf.url) {
            printjs({printable: pdf.url, type: 'pdf'});
        }
    } else {
        // Workaround for printing encrypted PDFs, we can't use the encrypted raw PDF data
        // so we will instead load all pages as images and then pass those images to printJS
        const pagePromises = [];
        for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
            pagePromises.push(
                pdf.document.getPage(pageNum).then(page => {
                    const viewport = page.getViewport({scale: 2});
                    const canvas = document.createElement('canvas');
                    const context = canvas.getContext('2d');
                    canvas.width = viewport.width;
                    canvas.height = viewport.height;

                    return page.render({canvasContext: context, viewport: viewport}).promise
                        .then(() => canvas.toDataURL('image/png'));
                })
            );

            // @ts-ignore
            Promise.all(pagePromises).then(imageDataArray => {
                // @ts-ignore
                printJS({printable: imageDataArray, type: 'image'});
            });
        }
    }
}

export function downloadDocument(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    if (pdf.url) {
        fetch(pdf.url).then(response => {
            if (response.ok) {
                response.blob().then(blob => {
                    saveAs(blob, pdf.filename ?? 'document.pdf');
                });
            }
        });
    }
}

function scrollToPage(id: string, pageNumber: number) {
    const container = document.getElementById(id);
    const targetPage = document.getElementById(`${id}-page-${pageNumber}`);
    if (container && targetPage) {
        container.scrollTo({
            top: targetPage.offsetTop - container.offsetTop,
            behavior: 'smooth'
        });
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

                // Render text layer
                const textLayer = document.getElementById(`${pdf.id}_text`) as HTMLDivElement;
                textLayer.replaceChildren();
                textLayer.style.left = pdf.canvas.offsetLeft + 'px';
                textLayer.style.top = pdf.canvas.offsetTop + 'px';
                textLayer.style.height = pdf.canvas.offsetHeight + 'px';
                textLayer.style.width = pdf.canvas.offsetWidth + 'px';

                const textLayerBuilder = new TextLayerBuilder({pdfPage})
                textLayerBuilder.div = textLayer;
                textLayerBuilder.render(viewport);

                if (pdf.queuedPage !== null) {
                    renderPdf(pdf);
                    pdf.queuedPage = null;
                }
            })
        })
    } else {
        const container = document.getElementById(pdf.id);
        container.innerHTML = '';

        // @ts-ignore
        pdfjs.getDocument(pdf.url).promise.then(async function (doc) {
            for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
                const page = await doc.getPage(pageNum);
                const viewport = page.getViewport({scale: pdf.scale, rotation: pdf.rotation});

                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');

                canvas.id = `${pdf.id}-page-${pageNum}`;
                canvas.classList.add('mudpdf_scroll_page');
                canvas.width = viewport.width;
                canvas.height = viewport.height;
                container.appendChild(canvas);

                await page.render({canvasContext: ctx, viewport}).promise;
            }
        })

        pdf.renderInProgress = false;
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

    dotnetReference.invokeMethodAsync('SetPdfViewerMetaData', {currentPage: pdf.currentPage, totalPages: pdf.pageCount});
}