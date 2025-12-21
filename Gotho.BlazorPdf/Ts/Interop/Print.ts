import printjs from "print-js"
import { Pdf } from "../PdfDocument/Pdf";

import DotNetObject = DotNet.DotNetObject;

export async function printDocument(dotnetReference: DotNetObject, id: string) {
    const pdf = Pdf.getPdf(id);
    const imageDataArray: string[] = [];

    for (let pageNum = 1; pageNum <= pdf.pageCount; pageNum++) {
        const page = await pdf.document!.getPage(pageNum);
        const scale = 2;
        const viewport = page.getViewport({scale});

        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d') as CanvasRenderingContext2D;
        canvas.width = viewport.width;
        canvas.height = viewport.height;

        await page.render({canvasContext: context, viewport}).promise;

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
    printjs({printable: imageDataArray, type: 'image'});
}