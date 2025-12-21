import { PDFPageProxy } from "pdfjs-dist";
import { Pdf } from "../PdfDocument/Pdf";
import {TextLayerBuilder} from 'pdfjs-dist/web/pdf_viewer.mjs';

export async function queuePdfRender(pdf: Pdf, pageNumber: number | null) {
    if (pdf.renderInProgress) {
        if (pageNumber !== null) {
            pdf.queuedPage = pageNumber;
        }
        return;
    }

    await renderPdf(pdf);
}

export async function renderPdf(pdf: Pdf) {
    pdf.renderInProgress = true;

    if (!pdf.scrollMode) {
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