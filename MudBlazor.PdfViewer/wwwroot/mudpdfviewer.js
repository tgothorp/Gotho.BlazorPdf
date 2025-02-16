"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Display = void 0;
exports.init = init;
var Display = /** @class */ (function () {
    function Display(portrait, scale, rotation, showThumbnails) {
        this.portrait = portrait;
        this.scale = scale;
        this.rotation = rotation;
        this.showThumbnails = showThumbnails;
    }
    Display.init = function (canvas) {
        this.instance = new Display(true, 1.0, 0, true);
    };
    return Display;
}());
exports.Display = Display;
function init(elementId) {
    // Setup the display class
    var element = document.getElementById(elementId);
    if (!element) {
        Display.init(element);
    }
    // Setup the PDF class
}
