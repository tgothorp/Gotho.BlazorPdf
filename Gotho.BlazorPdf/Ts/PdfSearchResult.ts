export class PdfSearchResult {
    public page: number;
    public index: number;
    public content: string;
    
    constructor(page: number, index: number, context: string) {
        this.page = page;
        this.index = index;
        this.content = context;
    }
}