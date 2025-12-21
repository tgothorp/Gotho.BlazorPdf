import {Pdf} from "../PdfDocument/Pdf";

import DotNetObject = DotNet.DotNetObject;

export function undoLastStroke(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.drawLayer.undoLastStroke();
}

export function clearStrokesForPage(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);
    pdf.drawLayer.clearPageStrokes();
}
