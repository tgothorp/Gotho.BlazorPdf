export class Display {

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
    }
}

export function init(elementId: string) {

    // Setup the display class
    const element = document.getElementById(elementId) as HTMLCanvasElement | null;
    if (!element) {
        Display.init(element)
    }
    
    // Setup the PDF class
}