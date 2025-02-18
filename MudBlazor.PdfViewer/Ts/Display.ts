export class Display {

    public static instances: {}

    constructor(
        public canvasId: string,
        public scale: number,
        public rotation: number,
        public singlePageMode: boolean,
        public canvas: HTMLCanvasElement
    ) {
    }

    public static initNewDisplay(canvasId: string, scale: number, rotation: number) {
        if (this.instances == undefined) {
            this.instances = {};
        }

        const canvas = this.getCanvas(canvasId);
        if (canvas) {
            this.instances[canvasId] = new Display(canvasId, scale, rotation, true, canvas);
        }
    }

    public static getDisplay(canvasId: string): Display {

        return Display.instances[canvasId];
    }

    public context(): CanvasRenderingContext2D {
        return this.canvas.getContext("2d");
    }

    public rotate(rotation: number) {
        if (rotation % 90 === 0)
            this.rotation = rotation;
    }

    public zoom(scale: number) {
        this.scale = scale;
    }

    private static getCanvas(canvasId: string | HTMLElement | HTMLCanvasElement | ArrayLike<any> | { canvas: HTMLCanvasElement })
        : HTMLCanvasElement | null {
        if (this.isDomSupported() && typeof canvasId === 'string') {
            const element = document.getElementById(canvasId);
            if (!element) {
                return null;
            }
            canvasId = element;
        } else if (canvasId && typeof (canvasId as any).length !== 'undefined') {
            // Support for array based queries: use the first element
            canvasId = (canvasId as any)[0];
        }

        if (canvasId && (canvasId as any).canvas !== undefined && (canvasId as any).canvas) {
            // Support for any object associated with a canvas (including a context2d)
            canvasId = (canvasId as any).canvas;
        }

        return canvasId instanceof HTMLCanvasElement ? canvasId : null;
    }

    private static isDomSupported(): boolean {
        return true;
    }
}