import {Pdf} from "../PdfDocument/Pdf";
import {PDFDocumentProxy} from "pdfjs-dist";

import DotNetObject = DotNet.DotNetObject;
import {goToPage} from "./Paging";

export async function renderThumbnails(dotnetReference: DotNetObject, pdf: Pdf) {
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