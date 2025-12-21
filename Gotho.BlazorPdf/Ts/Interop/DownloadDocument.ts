import FileSaver from "file-saver";
import {Pdf} from "../PdfDocument/Pdf";

import DotNetObject = DotNet.DotNetObject;

export async function downloadDocument(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);

    if (pdf.fileBytes) {
        const fileName = pdf.fileName ?? 'document.pdf';
        const blob = new Blob([pdf.fileBytes], { type: 'application/pdf' });
        FileSaver.saveAs(blob, fileName);
        return;
    }

    if (pdf.url) {
        fetch(pdf.url!).then(response => {
            if (response.ok) {
                response.blob().then(blob => {
                    FileSaver.saveAs(blob, pdf.fileName ?? 'document.pdf');
                });
            }
        });
    }
}