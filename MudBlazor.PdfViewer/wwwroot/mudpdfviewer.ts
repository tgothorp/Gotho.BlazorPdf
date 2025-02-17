class Display {

    public static instance: Display;

    public canvas: HTMLCanvasElement;

    constructor(
        public portrait: boolean,
        public scale: number,
        public rotation: number,
        public showThumbnails: boolean,
        public singlePageMode: boolean,
    ) {
    }

    public static init(canvas: HTMLCanvasElement) {
        this.instance = new Display(true, 1.0, 0, true, true);
        this.instance.canvas = canvas;
    }
    
    public rotateClockwise()
    {
        if (this.rotation >= 270) {
            this.rotation = 0;
            return;
        }

        this.rotation = this.rotation + 90;
    }
}

export function init(dotnetReference : any, elementId: string) {

    // Set up the display class
    const element = document.getElementById(elementId) as HTMLCanvasElement | null;
    if (element) {
        Display.init(element)
    }
    
    // Set up the PDF class
}

export function rotateClockwise() {
    Display.instance.rotateClockwise();
}