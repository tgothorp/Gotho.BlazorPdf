export class PdfDrawingLayer {

    public id: string;
    public enabled: boolean;
    public drawing: boolean;
    public canvasContext: CanvasRenderingContext2D;

    private canvas: HTMLCanvasElement | null = null;

    private boundMouseDown: (e: MouseEvent) => void;
    private boundMouseMove: (e: MouseEvent) => void;
    private boundMouseUp: () => void;
    private boundMouseLeave: () => void;
    
    constructor(id: string) {
        this.id = id;
        
        this.boundMouseDown = this.onMouseDown.bind(this);
        this.boundMouseMove = this.onMouseMove.bind(this);
        this.boundMouseUp = this.onMouseUp.bind(this);
        this.boundMouseLeave = this.onMouseLeave.bind(this);
    }

    public initialize(pdfCanvas: HTMLCanvasElement) {
        console.log("init draw layer...")
        
        const textLayer = document.getElementById(`${this.id}_text`)
        this.canvas = document.getElementById(`${this.id}_drawing`) as HTMLCanvasElement;

        // if (!this.canvas || !textLayer) 
        //     return;

        this.canvas.width = pdfCanvas.offsetWidth;
        this.canvas.height = pdfCanvas.offsetHeight;
        this.canvas.style.width = `${pdfCanvas.offsetWidth}px`;
        this.canvas.style.height = `${pdfCanvas.offsetHeight}px`;
        this.canvas.style.top = textLayer.style.top;
        this.canvas.style.left = textLayer.style.left;
        this.canvas.style.display = "none";

        this.canvasContext = this.canvas.getContext("2d");

        if (this.canvasContext) {
            this.canvasContext.strokeStyle = "#FF0000";
            this.canvasContext.lineWidth = 2;
        }
    }

    public enableLayer() {
        if (!this.canvas) 
            return;

        const textLayer = document.getElementById(`${this.id}_text`)
        textLayer.style.display = "none";

        this.enabled = true;
        this.canvas.style.removeProperty("display");

        this.canvas.addEventListener("mousedown", this.boundMouseDown);
        this.canvas.addEventListener("mousemove", this.boundMouseMove);
        this.canvas.addEventListener("mouseup", this.boundMouseUp);
        this.canvas.addEventListener("mouseleave", this.boundMouseLeave);
    }

    public disableLayer() {
        if (!this.canvas) 
            return;

        const textLayer = document.getElementById(`${this.id}_text`)
        textLayer.style.display = "";

        this.enabled = false;
        this.drawing = false;

        this.canvas.removeEventListener("mousedown", this.boundMouseDown);
        this.canvas.removeEventListener("mousemove", this.boundMouseMove);
        this.canvas.removeEventListener("mouseup", this.boundMouseUp);
        this.canvas.removeEventListener("mouseleave", this.boundMouseLeave);
    }

    private onMouseDown(e: MouseEvent) {
        if (!this.canvasContext || !this.canvas) return;

        this.drawing = true;
        const [x, y] = this.getMousePos(this.canvas, e);
        this.canvasContext.beginPath();
        this.canvasContext.moveTo(x, y);
    }

    private onMouseMove(e: MouseEvent) {
        if (!this.drawing || !this.canvasContext || !this.canvas) return;

        const [x, y] = this.getMousePos(this.canvas, e);
        this.canvasContext.lineTo(x, y);
        this.canvasContext.stroke();
    }

    private onMouseUp() {
        this.drawing = false;
    }

    private onMouseLeave() {
        this.drawing = false;
    }

    private getMousePos(canvas: HTMLCanvasElement, evt: MouseEvent): [number, number] {
        const rect = canvas.getBoundingClientRect();
        return [evt.clientX - rect.left, evt.clientY - rect.top];
    }
}