import {Pdf} from "./Pdf";
import {PdfState} from "./PdfState";
import {saveAs} from "file-saver"
import * as pdfjs from "pdfjs-dist"
import {TextLayerBuilder} from 'pdfjs-dist/web/pdf_viewer.mjs';

// @ts-ignore
import printjs from "print-js"

pdfjs.GlobalWorkerOptions.workerSrc = "./pdf.worker.min.mjs";

let workerInitialised = false;

// @ts-ignore
async function setupProjectWorker() {
    const response = await fetch('./pdf.worker.min.mjs');
    const workerCode = await response.text();

    const blob = new Blob([workerCode], {type: 'application/javascript'});
    const blobUrl = URL.createObjectURL(blob);

    pdfjs.GlobalWorkerOptions.workerSrc = blobUrl;
    workerInitialised = true;
}

export function initPdfViewer(dotnetReference: any, pdfDto: PdfState, singlePageMode: boolean, useProjectWorker: boolean) {
    console.log("Initializing PDF " + pdfDto.id);

    if (useProjectWorker && !workerInitialised) {
        setupProjectWorker();
    } else {
        workerInitialised = true;
    }

    if (pdfDto.url) {
        const pdf = new Pdf(pdfDto.id, pdfDto.scale, pdfDto.orientation, pdfDto.url, singlePageMode, pdfDto.source, pdfDto.password)

        pdfjs.getDocument(getDocumentInit(pdfDto)).promise.then(doc => {
            pdf.setDocument(doc)
            renderPdf(pdf)
            renderThumbnails(dotnetReference, pdf)

            dotnetReference.invokeMethodAsync('DocumentLoaded', {
                currentPage: pdf.currentPage,
                totalPages: pdf.pageCount
            });
        }).catch((err) => {
            dotnetReference.invokeMethodAsync('PdfViewerError', {name: err.name, message: err.message});
        })
    }
}

export function updatePdf(dotnetReference: any, pdfDto: PdfState) {
    const pdf = Pdf.getPdf(pdfDto.id)
    const previousPage = pdf.currentPage;
    pdf.updatePdf(pdfDto)
    pdf.drawLayer.updatePenSettings(pdfDto.penColor, pdfDto.penThickness);
    
    if (pdf.drawLayer.enabled !== pdfDto.drawLayerEnabled && pdf.singlePageMode) {
        if (pdfDto.drawLayerEnabled) {
            pdf.drawLayer.enable();
        } else {
            pdf.drawLayer.disable();
        }
    }

    if (!pdf.singlePageMode && pdf.currentPage !== previousPage) {
        scrollToPage(pdf.id, pdf.currentPage);
        updateMetadata(dotnetReference, pdf)

        return;
    }

    document.body.style.setProperty('--scale-factor', `${pdf.scale}`);
    queuePdfRender(pdf, null);
    updateMetadata(dotnetReference, pdf)
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

export async function printDocument(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    const imageDataArray: string[] = [];

    for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
        const page = await pdf.document.getPage(pageNum);
        const scale = 2;
        const viewport = page.getViewport({ scale });

        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        canvas.width = viewport.width;
        canvas.height = viewport.height;

        await page.render({ canvasContext: context, viewport }).promise;

        const pdfImage = context.canvas;

        // Create merged canvas
        const mergedCanvas = document.createElement('canvas');
        mergedCanvas.width = pdfImage.width;
        mergedCanvas.height = pdfImage.height;
        const mergedContext = mergedCanvas.getContext('2d');

        // Draw base PDF page
        mergedContext.drawImage(pdfImage, 0, 0);

        // Draw annotation layer
        const drawingLayer = pdf.drawLayer;
        const pageStrokes = drawingLayer.drawingStore[pageNum] || [];

        if (pageStrokes.length > 0) {
            const annotationCanvas = document.createElement('canvas');
            annotationCanvas.width = mergedCanvas.width;
            annotationCanvas.height = mergedCanvas.height;
            const annotationCtx = annotationCanvas.getContext('2d');

            for (const stroke of pageStrokes) {
                annotationCtx.beginPath();
                annotationCtx.strokeStyle = stroke.color;
                annotationCtx.lineWidth = stroke.thickness;

                stroke.points.forEach((p, i) => {
                    const x = p.x * annotationCanvas.width;
                    const y = p.y * annotationCanvas.height;
                    if (i === 0) {
                        annotationCtx.moveTo(x, y);
                    } else {
                        annotationCtx.lineTo(x, y);
                    }
                });

                annotationCtx.stroke();
            }

            mergedContext.drawImage(annotationCanvas, 0, 0);
        }

        // Add the result to the list
        imageDataArray.push(mergedCanvas.toDataURL('image/png'));
    }

    // Finally, print all pages
    printjs({ printable: imageDataArray, type: 'image' });
}

export function downloadDocument(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    if (pdf.url) {

        if (pdf.source == "base64") {

            let base64Data = pdf.url;

            if (pdf.url.indexOf('data:') === 0) {
                const split = pdf.url.split(',');
                base64Data = split.length > 1 ? split[1] : '';
            }

            try {
                const byteCharacters = atob(base64Data);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);
                const blob = new Blob([byteArray], {type: 'application/pdf'});

                saveAs(blob, "document.pdf");
            } catch (e) {
                console.error('Failed to decode base64 PDF:', e);
            }

        } else {
            fetch(pdf.url).then(response => {
                if (response.ok) {
                    response.blob().then(blob => {
                        saveAs(blob, pdf.filename ?? 'document.pdf');
                    });
                }
            });
        }
    }
}

export function undoLastStroke(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.drawLayer.undoLastStroke();
}

export function clearStrokesForPage(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.drawLayer.clearPageStrokes();
}

export async function viewMetadata(dotnetReference: any, id: string) {
    const pdf = Pdf.getPdf(id);
    
    const data = await pdf.getMetadata();
    dotnetReference.invokeMethodAsync('PdfMetadata', data);
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

            // Update draw layer
            pdf.drawLayer.updateCanvas(pdf.currentPage, viewport.height, viewport.width, pdf.canvas.offsetLeft, pdf.canvas.offsetTop, pdf.rotation);
        })
    } else {
        const container = document.getElementById(pdf.id);
        container.innerHTML = '';

        // @ts-ignore
        pdfjs.getDocument(pdf.url).promise.then(async function (doc) {
            let fixedScale = pdf.scale;
            let fixedRotation = pdf.rotation;

            // @ts-ignore
            async function renderPage(pageNum: number) {
                const page = await doc.getPage(pageNum);
                const viewport = page.getViewport({scale: fixedScale, rotation: fixedRotation});

                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');

                canvas.id = `${pdf.id}-page-${pageNum}`;
                canvas.classList.add('blazorpdf__scroll-page');
                canvas.width = viewport.width;
                canvas.height = viewport.height;
                container.appendChild(canvas);

                await page.render({canvasContext: ctx, viewport}).promise;
            }

            // Render pages sequentially
            for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
                await renderPage(pageNum);
            }

            pdf.renderInProgress = false;
        });
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
        thumbCanvas.classList.add('blazorpdf-pdf__thumbnails-thumbnail');

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

    dotnetReference.invokeMethodAsync('SetPdfViewerMetaData', {
        currentPage: pdf.currentPage,
        totalPages: pdf.pageCount
    });
}

function getDocumentInit(pdfDto: PdfState) {
    let documentInit: any = {};

    if (pdfDto.source == "Base64")
        documentInit.data = atob(pdfDto.url);
    else
        documentInit.url = pdfDto.url;

    if (pdfDto.password)
        documentInit.password = pdfDto.password;

    return documentInit;
}