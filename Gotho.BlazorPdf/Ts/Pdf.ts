import {PDFDocumentProxy, getFilenameFromUrl} from "pdfjs-dist"
import {PdfState} from "./PdfState";
import {PdfDrawLayer} from "./PdfDrawLayer";
import {PdfMetadata} from "./PdfMetadata";

const pdfInstances = {}

interface PdfJsMetadataInfo {
    Author?: string;
    Creator?: string;
    Keywords?: string;
    Producer?: string;
    Subject?: string;
    Title?: string;
    PDFFormatVersion?: string;
    CreationDate?: string;
    ModDate?: string;
    Custom?: any
}

interface PdfJsMetadata {
    info: PdfJsMetadataInfo;
    metadata: any;
}

export class Pdf {

    public id: string;
    public canvas: any;
    public scale: number;
    public rotation: number;
    public url: string;
    public filename: string;
    public document: PDFDocumentProxy | null;
    public metadata: PdfMetadata | null;

    public renderInProgress: boolean;
    public singlePageMode: boolean;
    public pageCount: number;
    public currentPage: number;
    public previousPage: number;
    public queuedPage: number | null;
    public password: string | null;
    public source: string;
    
    public drawLayer: PdfDrawLayer;

    constructor(id: string, scale: number, rotation: number, url: string, singlePageMode: boolean, source: string, password: string | null = null) {
        this.id = id;
        this.canvas = Pdf.getCanvas(id);
        this.scale = scale;
        this.rotation = rotation;
        this.url = url;
        this.filename = getFilenameFromUrl(url)
        this.document = null;
        this.metadata = null;
        this.renderInProgress = false;
        this.singlePageMode = singlePageMode;
        this.pageCount = 0;
        this.currentPage = 1;
        this.previousPage = 1;
        this.queuedPage = null;
        this.source = source.toLowerCase();
        this.password = password
        this.drawLayer = new PdfDrawLayer(id);

        // @ts-ignore
        pdfInstances[this.id] = this;
    }

    public static getPdf(id: string): Pdf {
        const canvas = this.getCanvas(id);
        return Object.values(pdfInstances).filter((c: any) => c.canvas === canvas).pop() as Pdf;
    }

    public updatePdf(dto: PdfState)
    {
        this.rotation = dto.orientation;
        this.scale = dto.scale;
        this.previousPage = this.currentPage;
        this.currentPage = dto.currentPage;
    }

    public setDocument(doc: PDFDocumentProxy) {
        this.document = doc;
        this.pageCount = doc.numPages;
    }

    public gotoPage(pageNumber: number): boolean {
        if (this.document == null || pageNumber < 1 || pageNumber > this.pageCount) {
            return false;
        }

        this.currentPage = pageNumber;
        return true;
    }

    public rotate(rotation: number) {
        if (rotation % 90 === 0)
            this.rotation = rotation;
    }

    public zoom(scale: number) {
        this.scale = scale;
    }

    public async getMetadata() : Promise<PdfMetadata> {
        if (this.metadata !== null)
            return this.metadata;
        
        const data = await this.document!.getMetadata() as PdfJsMetadata;
        const custom: Record<string, string> = {};
        
        if (data.info.Custom)
        {
            // @ts-ignore
            for (const [key, value] of Object.entries(data.info.Custom)) {
                if (value != null) {
                    custom[key] = String(value);
                }
            }
        }

        this.metadata = new PdfMetadata(
            data.info.Author,
            data.info.Creator,
            data.info.Keywords,
            data.info.Producer,
            data.info.Subject,
            data.info.Title,
            data.info.PDFFormatVersion,
            this.parsePdfDate(data.info.CreationDate),
            this.parsePdfDate(data.info.ModDate),
            custom,
        );

        return this.metadata;
    }
    
    public getCanvasContext(): any {
        return this.canvas.getContext("2d");
    }

    private static getCanvas(id: any) {
        if (this.isDomSupported() && typeof id === 'string') {
            id = document.getElementById(id);
        } else if (id && id.length) {
            // support for array based queries
            id = id[0];
        }

        if (id && id.canvas !== undefined && id.canvas) {
            // support for any object associated to a canvas (including a context2d)
            id = id.canvas;
        }

        return id;
    }

    private static isDomSupported(): boolean {
        return true;
    }

    private parsePdfDate(pdfDateStr?: string): Date | null {
        // @ts-ignore
        if (!pdfDateStr || !pdfDateStr.startsWith('D:')) return null;

        const regex = /^D:(\d{4})(\d{2})?(\d{2})?(\d{2})?(\d{2})?(\d{2})?([Z\+\-])?(\d{2})'?(\d{2})'?/;
        const match = regex.exec(pdfDateStr);

        if (!match) return null;

        const [
            ,
            year,
            month = '01',
            day = '01',
            hour = '00',
            minute = '00',
            second = '00',
            tzSign,
            tzHour,
            tzMin
        ] = match;

        const dateStr = `${year}-${month}-${day}T${hour}:${minute}:${second}`;

        if (tzSign && tzSign !== 'Z' && tzHour && tzMin) {
            return new Date(`${dateStr}${tzSign}${tzHour}:${tzMin}`);
        }

        return new Date(dateStr);
    }
}