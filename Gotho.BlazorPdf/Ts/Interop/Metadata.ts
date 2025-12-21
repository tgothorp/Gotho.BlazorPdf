import { Pdf } from "../PdfDocument/Pdf";

import DotNetObject = DotNet.DotNetObject;

export async function viewMetadata(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);

    const data = await pdf.getMetadata();
    await dotnetReference.invokeMethodAsync('PdfMetadata', data);
}

export async function updateMetadata(dotnetReference: DotNetObject, pdf: Pdf) {
    if (dotnetReference == null)
        return;

    await dotnetReference.invokeMethodAsync('SetPdfViewerMetaData', {
        currentPage: pdf.currentPage,
        totalPages: pdf.pageCount
    });
}