export class PdfSearchResult {
    public page: number;
    public index: number;
    
    constructor(page: number, index: number) {
        this.page = page;
        this.index = index;
    }
}