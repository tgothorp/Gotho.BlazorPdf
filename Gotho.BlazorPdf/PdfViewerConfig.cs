namespace Gotho.BlazorPdf;

public class PdfViewerConfig
{
    public Label Labels { get; set; } = new();
    
    public Color Colors { get; set; } = new();

    public class Label
    {
        public string ToggleThumbnails { get; set; } = "Show Thumbnails";
        public string PreviousPage { get; set; } = "Previous Page";
        public string NextPage { get; set; } = "Next Page";
        public string PageOf { get; set; } = "of";
        public string ZoomIn { get; set; } = "Zoom In";
        public string ZoomOut { get; set; } = "Zoom Out";
        public string OpenMenu { get; set; } = "Menu";
        public string RotateClockwise { get; set; } = "Rotate Clockwise";
        public string RotateCounterclockwise { get; set; } = "Rotate Counterclockwise";
        public string SwitchOrientation { get; set; } = "Switch Orientation";
        public string FirstPage { get; set; } = "First Page";
        public string LastPage { get; set; } = "Last Page";
        public string ResetZoom { get; set; } = "Reset Zoom";
        public string PrintDocument { get; set; } = "Print Document";
        public string DownloadDocument { get; set; } = "Download Document";
    }
    
    public class Color
    {
        public string Toolbar { get; set; } = "#e8e7ea";
        public string Loader { get; set; } = "#F009AF";
        public string Background { get; set; } = "#161719";
        public string Icon { get; set; } = "#161719";
    }
}