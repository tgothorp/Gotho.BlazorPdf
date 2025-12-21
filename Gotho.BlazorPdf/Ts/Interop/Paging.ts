import {Pdf} from "../PdfDocument/Pdf";
import {queuePdfRender} from "./RenderPdf";
import {updateMetadata} from "./Metadata";

import DotNetObject = DotNet.DotNetObject;

export async function goToPage(dotnetReference: DotNetObject, id: string, pageNumber: number) {
    const pdf = Pdf.getPdf(id);
    if (pdf.gotoPage(pageNumber)) {
        if (!pdf.scrollMode) {
            await queuePdfRender(pdf, null);
            await updateMetadata(dotnetReference, pdf);
        } else {
            scrollToPage(id, pageNumber);
            await updateMetadata(dotnetReference, pdf);
        }
    }
}

export function scrollToPage(id: string, pageNumber: number) {
    const container = document.getElementById(id);
    const targetPage = document.getElementById(`${id}-page-${pageNumber}`);
    if (container && targetPage) {
        container.scrollTo({
            top: targetPage.offsetTop - container.offsetTop,
            behavior: 'smooth'
        });
    }
}