import {Pdf} from "./Pdf";
import {PdfState} from "./PdfState";
import {TextLayerBuilder} from 'pdfjs-dist/web/pdf_viewer.mjs';
import printjs from "print-js"
import DotNetObject = DotNet.DotNetObject;
import FileSaver from "file-saver";
import {GlobalWorkerOptions, getDocument, PDFPageProxy, PDFDocumentProxy} from "pdfjs-dist";

GlobalWorkerOptions.workerSrc = "./pdf.worker.min.mjs";
let workerInitialised = false;

/**
 * This is a work-around for .NET MAUI, the MAUI browser used by Blazor cannot load
 * the required worker directly so we must grab it via a fetch().
 */
async function setupProjectWorker() {
    const response = await fetch('./pdf.worker.min.mjs');
    const workerCode = await response.text();

    const blob = new Blob([workerCode], {type: 'application/javascript'});
    GlobalWorkerOptions.workerSrc = URL.createObjectURL(blob);
    workerInitialised = true;
}

export async function initPdfViewer(dotnetReference: DotNetObject, pdfDto: PdfState, singlePageMode: boolean, useProjectWorker: boolean) : Promise<void> {
    console.log("Initializing PDF " + pdfDto.id);

    if (useProjectWorker && !workerInitialised) {
        await setupProjectWorker();
    } else {
        workerInitialised = true;
    }

    if (pdfDto.url) {
        const pdf = new Pdf(pdfDto.id as string, pdfDto.scale, pdfDto.orientation, pdfDto.url, singlePageMode, pdfDto.source, pdfDto.password)

        try {
            const loadedDocument = await getDocument(getDocumentInit(pdfDto)).promise;
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
}

export async function updatePdf(dotnetReference: DotNetObject, pdfDto: PdfState) {
    closeMenu();
    const pdf = Pdf.getPdf(pdfDto.id as string)
    const previousPage = pdf.currentPage;
    pdf.updatePdf(pdfDto)
    pdf.drawLayer.updatePenSettings(pdfDto.penColor, pdfDto.penThickness);
    
    if (pdfDto.searchQuery && pdfDto.searchQuery !== pdf.previousQuery) {
        const results = pdf.search(pdfDto.searchQuery);
        const blob = new Blob([JSON.stringify(results)], { type: 'application/json' });
        const streamRef = DotNet.createJSStreamReference(blob);
        await dotnetReference.invokeMethodAsync('SearchResultsFromStream', streamRef);
    }

    if (pdf.drawLayer.enabled !== pdfDto.drawLayerEnabled && pdf.singlePageMode) {
        if (pdfDto.drawLayerEnabled) {
            pdf.drawLayer.enable();
        } else {
            pdf.drawLayer.disable();
        }
    }

    if (!pdf.singlePageMode && pdf.currentPage !== pdf.previousPage) {
        scrollToPage(pdf.id, pdf.currentPage);
        await updateMetadata(dotnetReference, pdf)

        return;
    }

    document.body.style.setProperty('--scale-factor', `${pdf.scale}`);
    await queuePdfRender(pdf, null);
    await updateMetadata(dotnetReference, pdf)
}

export async function clearSearchResults(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.clearSearchResults();

    await renderPdf(pdf);
}

export async function goToPage(dotnetReference: DotNetObject, id: string, pageNumber: number) {
    const pdf = Pdf.getPdf(id);
    if (pdf.gotoPage(pageNumber)) {
        if (pdf.singlePageMode) {
            await queuePdfRender(pdf, null);
            await updateMetadata(dotnetReference, pdf);
        } else {
            scrollToPage(id, pageNumber);
            await updateMetadata(dotnetReference, pdf);
        }
    }
}

export async function printDocument(dotnetReference: DotNetObject, id: string) {
    closeMenu();
    const pdf = Pdf.getPdf(id);
    const imageDataArray: string[] = [];

    for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
        const page = await pdf.document!.getPage(pageNum);
        const scale = 2;
        const viewport = page.getViewport({ scale });

        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d') as CanvasRenderingContext2D;
        canvas.width = viewport.width;
        canvas.height = viewport.height;

        await page.render({ canvasContext: context, viewport }).promise;

        const pdfImage = context.canvas;

        // Create merged canvas
        const mergedCanvas = document.createElement('canvas');
        mergedCanvas.width = pdfImage.width;
        mergedCanvas.height = pdfImage.height;
        const mergedContext = mergedCanvas.getContext('2d') as CanvasRenderingContext2D;

        // Draw base PDF page
        mergedContext.drawImage(pdfImage, 0, 0);

        // Draw annotation layer
        const drawingLayer = pdf.drawLayer;
        const pageStrokes = drawingLayer.drawingStore[pageNum] || [];

        if (pageStrokes.length > 0) {
            const annotationCanvas = document.createElement('canvas');
            annotationCanvas.width = mergedCanvas.width;
            annotationCanvas.height = mergedCanvas.height;
            const annotationCtx = annotationCanvas.getContext('2d') as CanvasRenderingContext2D;

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
        imageDataArray.push(mergedCanvas.toDataURL('image/png'));
    }
    printjs({ printable: imageDataArray, type: 'image' });
}

export async function downloadDocument(dotnetReference: DotNetObject, id: string) {
    closeMenu();
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

                FileSaver.saveAs(blob, "document.pdf");
            } catch (e) {
                console.error('Failed to decode base64 PDF:', e);
            }

        } else {
            fetch(pdf.url).then(response => {
                if (response.ok) {
                    response.blob().then(blob => {
                        FileSaver.saveAs(blob, pdf.filename ?? 'document.pdf');
                    });
                }
            });
        }
    }
}

export function undoLastStroke(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.drawLayer.undoLastStroke();
}

export function clearStrokesForPage(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.drawLayer.clearPageStrokes();
}

export async function viewMetadata(dotnetReference: DotNetObject, id: string) {
    closeMenu();
    const pdf = Pdf.getPdf(id);
    
    const data = await pdf.getMetadata();
    await dotnetReference.invokeMethodAsync('PdfMetadata', data);
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

async function queuePdfRender(pdf: Pdf, pageNumber: number | null) {
    if (pdf.renderInProgress) {
        if (pageNumber !== null) {
            pdf.queuedPage = pageNumber;
        }
        return;
    }

    await renderPdf(pdf);
}

async function renderPdf(pdf: Pdf) {
    pdf.renderInProgress = true;

    if (pdf.singlePageMode) {
        pdf.document!.getPage(pdf.currentPage).then(async (pdfPage) => {
            const viewport = pdfPage.getViewport({scale: pdf.scale, rotation: pdf.rotation});
            pdf.canvas.width = viewport.width;
            pdf.canvas.height = viewport.height;

            const renderData = {
                canvasContext: pdf.getCanvasContext(),
                viewport: viewport
            }

            const renderTask = pdfPage.render(renderData);
            await renderTask.promise;

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
                
                // Wait for text layer to render before applying highlights
                textLayerBuilder.render(viewport).then(() => {
                    if (pdf.previousQuery === null)
                        return;

                    const spans = textLayer.querySelectorAll('span');
                    const query = pdf.previousQuery!.toLowerCase();
                    let resultIndex = -1;

                    Array.from(spans).forEach((span, index) => {
                        const text = span.textContent || "";
                        const matchIndex = text.toLowerCase().indexOf(query);
                        if (matchIndex === -1) return;

                        resultIndex += 1;
                        const before = text.slice(0, matchIndex);
                        const match = text.slice(matchIndex, matchIndex + query.length);
                        const after = text.slice(matchIndex + query.length);
                        
                        if (pdf.activeSearchIndex === resultIndex) {
                            span.innerHTML = `${before}<mark class="active">${match}</mark>${after}`;
                        } else {
                            span.innerHTML = `${before}<mark>${match}</mark>${after}`;
                        }
                    });
                });


                if (pdf.queuedPage !== null) {
                    renderPdf(pdf);
                    pdf.queuedPage = null;
                }
            })

            // Update draw layer
            pdf.drawLayer.updateCanvas(pdf.currentPage, viewport.height, viewport.width, pdf.canvas.offsetLeft, pdf.canvas.offsetTop, pdf.rotation);
        })
    } else {
        const container = document.getElementById(pdf.id) as HTMLElement;
        container.innerHTML = '';
        
        let fixedScale = pdf.scale;
        let fixedRotation = pdf.rotation;

        async function renderPage(page: PDFPageProxy): Promise<void> {
            const viewport = page.getViewport({scale: fixedScale, rotation: fixedRotation});
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d') as CanvasRenderingContext2D;

            canvas.id = `${pdf.id}-page-${page.pageNumber}`;
            canvas.classList.add('blazorpdf__scroll-page');
            canvas.width = viewport.width;
            canvas.height = viewport.height;
            container.appendChild(canvas);

            const renderTask = page.render({canvasContext: ctx, viewport});
            await renderTask.promise.then(async () => {

                // Effective yet slightly cursed way to ensure correct text layer 
                // div offset, 16 is the pixel gap between individual pages
                const top = (viewport.height * (page.pageNumber - 1)) + (16 * (page.pageNumber - 1));

                const textDiv = document.createElement('div')
                textDiv.classList.add('textLayer')
                textDiv.style.left = '0px';
                textDiv.style.top = top + 'px';
                textDiv.style.height = pdf.canvas.offsetHeight + 'px';
                textDiv.style.width = pdf.canvas.offsetWidth + 'px';
                container.appendChild(textDiv);

                const textLayerBuilder = new TextLayerBuilder({pdfPage: page})
                textLayerBuilder.div = textDiv;
                textLayerBuilder.pdfPage = page;
                
                // Wait for text layer to render before applying highlights
                textLayerBuilder.render(viewport).then(() => {
                    if (pdf.previousQuery === null)
                        return;

                    const spans = textDiv.querySelectorAll('span');
                    const query = pdf.previousQuery!.toLowerCase();

                    Array.from(spans).forEach(span => {
                        const text = span.textContent || "";
                        const matchIndex = text.toLowerCase().indexOf(query);
                        if (matchIndex === -1) return;

                        const before = text.slice(0, matchIndex);
                        const match = text.slice(matchIndex, matchIndex + query.length);
                        const after = text.slice(matchIndex + query.length);

                        span.innerHTML = `${before}<mark>${match}</mark>${after}`;
                    });
                });
            });
        }

        for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
            const page = await pdf.document!.getPage(pageNum);
            await renderPage(page);
        }

        pdf.renderInProgress = false;
    }
}

async function renderThumbnails(dotnetReference: DotNetObject, pdf: Pdf) {
    const sidebar = document.getElementById(`${pdf.id}_thumbs`) as HTMLElement;
    sidebar.innerHTML = '';

    for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
        const doc = pdf.document as PDFDocumentProxy;
        const page = await doc.getPage(pageNum);

        let viewport = page.getViewport({scale: 0.2});
        let thumbCanvas = document.createElement('canvas');
        let thumbCtx = thumbCanvas.getContext('2d') as CanvasRenderingContext2D;

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

async function updateMetadata(dotnetReference: DotNetObject, pdf: Pdf) {
    if (dotnetReference == null)
        return;

    await dotnetReference.invokeMethodAsync('SetPdfViewerMetaData', {
        currentPage: pdf.currentPage,
        totalPages: pdf.pageCount
    });
}

function base64ToUint8Array(base64: string): Uint8Array {
    const raw = atob(base64);
    const uint8 = new Uint8Array(raw.length);
    for (let i = 0; i < raw.length; i++) {
        uint8[i] = raw.charCodeAt(i);
    }
    return uint8;
}

function getDocumentInit(pdfDto: PdfState) {
    let documentInit: any = {};

    if (pdfDto.source === "Base64") {
        documentInit.data = base64ToUint8Array(pdfDto.url as string);
    } else {
        documentInit.url = pdfDto.url;
    }

    if (pdfDto.password)
        documentInit.password = pdfDto.password;

    return documentInit;
}

function closeMenu() {
    const checkbox = document.getElementById('menu-toggle') as HTMLInputElement | null;
    if (checkbox && checkbox.type === 'checkbox') {
        checkbox.checked = false;
    }
}