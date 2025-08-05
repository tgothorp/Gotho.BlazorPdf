export class PdfState {
    public id: string | null = null;
    public url: string | null = null;
    public orientation: number = 0;
    public scale: number = 1;
    public currentPage: number = 1;
    public singlePageMode: boolean = true;
    public password: string | null = null;
    public source: string = "Url";
    public drawLayerEnabled: boolean = false;
    public penColor: string = "#000000";
    public penThickness: number = 1
}