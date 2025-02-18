import {PDFDocumentProxy} from "pdfjs-dist"

export class MudPdf {

    public static instance: MudPdf;

    public pdfDocument: PDFDocumentProxy;

    constructor(
        public url: string,
        public renderInProgress: boolean = false,
        public pageCount: number = 0,
        public currentPage: number = 1,
        public queuedPage: number = null,
    ) {
    }

    public static init(url: string) {
        this.instance = new MudPdf(url);
    }

    public setPdfDocument(pdfDocument: PDFDocumentProxy) {
        this.pdfDocument = pdfDocument;
        this.pageCount = pdfDocument.numPages;
        this.currentPage = 1;
    }

    public firstPage(): boolean {
        if (this.pdfDocument == null || this.currentPage == 1) {
            return false;
        }

        if (this.currentPage > 1)
            this.currentPage = 1;

        return true;
    }

    public lastPage() : boolean {
        if (this.pdfDocument == null || (this.currentPage == 1 && this.currentPage === this.pageCount)) {
            return false;
        }

        if (this.currentPage < this.pageCount)
            this.currentPage = this.pageCount;
        
        return true;
    }

    public previousPage(): boolean {
        if (this.pdfDocument == null || this.currentPage === 0 || this.currentPage === 1) {
            return false;
        }
        
        if (this.currentPage > 0) {
            this.currentPage -= 1;
        }
        
        return true;
    }
    
    public nextPage(): boolean {
        if (this.pdfDocument == null || this.currentPage === this.pageCount)
            return false;

        if (this.currentPage < this.pageCount)
            this.currentPage += 1;
        
        return true;
    }
    
    public gotoPage(pageNumber: number) : boolean {
        if (this.pdfDocument == null || pageNumber < 1 || pageNumber > this.pageCount) {
            return false;
        }
        
        this.currentPage = pageNumber;
        return true;
    }
}