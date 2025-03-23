namespace Gotho.BlazorPdf;

public class PdfViewerConfig
{
    /// <summary>
    /// Indicates whether the PDF.js worker script should be loaded from a local project path using a Blob URL,
    /// instead of the default method used by the browser.
    /// </summary>
    /// <remarks>
    /// This setting exists to support platforms where standard Web Workers cannot be loaded using script URLs,
    /// such as in .NET MAUI Blazor Hybrid applications.
    /// 
    /// <para><b>Default:</b> <c>false</c></para>
    /// </remarks>
    public bool UseProjectWorker { get; set; } = false;
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