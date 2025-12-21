import {PdfState} from "../PdfDocument/PdfState";
import {Pdf} from "../PdfDocument/Pdf";

import {queuePdfRender} from "./RenderPdf";
import {scrollToPage} from "./Paging";
import {updateMetadata} from "./Metadata";

import DotNetObject = DotNet.DotNetObject;

export async function updatePdf(dotnetReference: DotNetObject, pdfDto: PdfState) {
    const pdf = Pdf.getPdf(pdfDto.id as string)
    
    pdf.updatePdf(pdfDto)
    pdf.drawLayer.updatePenSettings(pdfDto.penColor, pdfDto.penThickness);

    if (pdfDto.searchQuery && pdfDto.searchQuery !== pdf.previousQuery) {
        const results = pdf.search(pdfDto.searchQuery);
        const blob = new Blob([JSON.stringify(results)], {type: 'application/json'});
        const streamRef = DotNet.createJSStreamReference(blob);
        await dotnetReference.invokeMethodAsync('SearchResultsFromStream', streamRef);
    }

    if (pdf.drawLayer.enabled !== pdfDto.drawLayerEnabled && !pdf.scrollMode) {
        if (pdfDto.drawLayerEnabled) {
            pdf.drawLayer.enable();
        } else {
            pdf.drawLayer.disable();
        }
    }

    if (pdf.scrollMode && pdf.currentPage !== pdf.previousPage) {
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

    await queuePdfRender(pdf, null);
}