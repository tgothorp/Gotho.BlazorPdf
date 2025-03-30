interface Stroke {
    color: string;
    thickness: number;
    points: { x: number; y: number }[];
}

export class PdfDrawLayer {
    public drawingStore: Record<number, Stroke[]> = {};
    public canvas: HTMLCanvasElement;
    public canvasContext: CanvasRenderingContext2D;
    public enabled: boolean;
    public drawing: boolean;

    private id: string;
    private strokes: Stroke[] = [];
    private currentStroke: Stroke | null = null;
    private rotation: number;
    private penColor: string = "#000000";
    private penThickness: number = 2;

    private currentPage: number = 1;

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

        this.canvas = document.getElementById(`${id}_drawing`) as HTMLCanvasElement;
        if (this.canvas) {
            this.canvasContext = this.canvas.getContext("2d");
            this.canvasContext.strokeStyle = this.penColor;
            this.canvasContext.lineWidth = this.penThickness;

            this.canvas.addEventListener("mousedown", this.boundMouseDown);
            this.canvas.addEventListener("mousemove", this.boundMouseMove);
            this.canvas.addEventListener("mouseup", this.boundMouseUp);
            this.canvas.addEventListener("mouseleave", this.boundMouseLeave);

            this.canvas.style.display = "none";
        }
    }

    public enable() {
        this.enabled = true;
        this.canvas.style.removeProperty("display");

        // Disable text layer temporarily
        const textLayer = document.getElementById(`${this.id}_text`)
        if (textLayer) {
            textLayer.style.display = "none";
        }

        this.canvas.addEventListener("mousedown", this.boundMouseDown);
        this.canvas.addEventListener("mousemove", this.boundMouseMove);
        this.canvas.addEventListener("mouseup", this.boundMouseUp);
        this.canvas.addEventListener("mouseleave", this.boundMouseLeave);
    }

    public disable() {
        this.enabled = false;
        this.drawing = false;

        // Reenable text layer
        const textLayer = document.getElementById(`${this.id}_text`)
        if (textLayer) {
            textLayer.style.removeProperty("display");
        }

        this.canvas.removeEventListener("mousedown", this.boundMouseDown);
        this.canvas.removeEventListener("mousemove", this.boundMouseMove);
        this.canvas.removeEventListener("mouseup", this.boundMouseUp);
        this.canvas.removeEventListener("mouseleave", this.boundMouseLeave);
    }

    public updatePenSettings(penColor: string, penThickness: number) {
        this.penColor = penColor;
        this.penThickness = penThickness;

        this.canvas = document.getElementById(`${this.id}_drawing`) as HTMLCanvasElement;
        if (this.canvas) {
            this.canvasContext = this.canvas.getContext("2d");
            this.canvasContext.strokeStyle = penColor;
            this.canvasContext.lineWidth = penThickness;
        }
    }

    public updateCanvas(pageNumber: number,
                        height: number,
                        width: number,
                        offsetLeft: number,
                        offsetTop: number,
                        rotation: number) {

        // Clamp rotation to 0, 90, 180 or 270
        this.rotation = ((rotation % 360) + 360) % 360;

        if (this.currentStroke) {
            this.strokes.push(this.currentStroke);
            this.currentStroke = null;
        }

        this.drawingStore[this.currentPage] = [...this.strokes];
        this.currentPage = pageNumber;
        this.strokes = this.drawingStore[pageNumber] ? [...this.drawingStore[pageNumber]] : [];

        if (this.canvas) {
            this.canvas.width = width;
            this.canvas.height = height;
            this.canvas.style.width = width + 'px';
            this.canvas.style.height = height + 'px';
            this.canvas.style.left = offsetLeft + 'px';
            this.canvas.style.top = offsetTop + 'px';
            this.redrawStrokes();
        }
    }

    public undoLastStroke() {
        if (this.strokes.length > 0) {
            this.strokes.pop();
            this.redrawStrokes();
        }
    }

    public clearPageStrokes() {
        this.strokes = [];
        this.drawingStore[this.currentPage] = [];
        this.redrawStrokes();
    }

    public getAllStrokes(): Record<number, Stroke[]> {
        if (this.currentStroke) {
            this.strokes.push(this.currentStroke);
            this.currentStroke = null;
        }

        this.drawingStore[this.currentPage] = [...this.strokes];
        return this.drawingStore;
    }

    public loadAllStrokes(data: Record<number, Stroke[]>) {
        this.drawingStore = data;
        this.strokes = this.drawingStore[this.currentPage] ?? [];
        this.redrawStrokes();
    }

    private redrawStrokes() {
        if (!this.canvasContext || !this.canvas) return;

        this.canvasContext.clearRect(0, 0, this.canvas.width, this.canvas.height);

        const w = this.canvas.width;
        const h = this.canvas.height;

        for (const stroke of this.strokes) {
            this.canvasContext.beginPath();
            this.canvasContext.strokeStyle = stroke.color;
            this.canvasContext.lineWidth = stroke.thickness;

            stroke.points.forEach((p, i) => {
                const [rx, ry] = this.rotatePoint(p.x, p.y, this.rotation);
                const x = rx * w;
                const y = ry * h;

                if (i === 0) {
                    this.canvasContext.moveTo(x, y);
                } else {
                    this.canvasContext.lineTo(x, y);
                }
            });

            this.canvasContext.stroke();
        }
    }

    private onMouseDown(e: MouseEvent) {
        if (!this.canvasContext || !this.canvas) return;

        this.canvasContext.strokeStyle = this.penColor;
        this.canvasContext.lineWidth = this.penThickness;

        this.drawing = true;
        const [x, y] = this.getNormalizedMousePos(this.canvas, e);
        this.canvasContext.beginPath();
        this.canvasContext.moveTo(x * this.canvas.width, y * this.canvas.height);

        this.currentStroke = {
            color: this.penColor,
            thickness: this.penThickness,
            points: [{x, y}]
        };
    }

    private onMouseMove(e: MouseEvent) {
        if (!this.drawing || !this.canvasContext || !this.canvas) return;

        const [x, y] = this.getNormalizedMousePos(this.canvas, e);
        this.canvasContext.lineTo(x * this.canvas.width, y * this.canvas.height);
        this.canvasContext.stroke();

        this.currentStroke?.points.push({x, y});
    }

    private onMouseUp() {
        this.drawing = false;
        if (this.currentStroke) {
            this.strokes.push(this.currentStroke);
            this.currentStroke = null;
        }
    }

    private onMouseLeave() {
        this.drawing = false;
    }

    private getNormalizedMousePos(canvas: HTMLCanvasElement, evt: MouseEvent): [number, number] {
        const rect = canvas.getBoundingClientRect();
        const x = (evt.clientX - rect.left) / canvas.width;
        const y = (evt.clientY - rect.top) / canvas.height;
        return [x, y];
    }

    private rotatePoint(x: number, y: number, rotation: number): [number, number] {
        switch (rotation) {
            case 90:
                return [1 - y, x];
            case 180:
                return [1 - x, 1 - y];
            case 270:
                return [y, 1 - x];
            default:
                return [x, y];
        }
    }
}