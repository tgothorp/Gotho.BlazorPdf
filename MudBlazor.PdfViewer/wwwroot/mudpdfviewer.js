class Display {
    constructor(portrait, scale, rotation, showThumbnails, singlePageMode) {
        this.portrait = portrait;
        this.scale = scale;
        this.rotation = rotation;
        this.showThumbnails = showThumbnails;
        this.singlePageMode = singlePageMode;
    }
    static init(canvas) {
        this.instance = new Display(true, 1.0, 0, true, true);
    }
}
export function init(dotnetReference, elementId) {
    // Setup the display class
    const element = document.getElementById(elementId);
    if (element) {
        Display.init(element);
    }
    // Setup the PDF class
}
