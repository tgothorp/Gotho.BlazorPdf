namespace Gotho.BlazorPdf.MudBlazor;

public class MudPdfViewerConfig
{
    public PdfViewerConfig.Label Labels { get; set; } = new();
    public Color Colors { get; set; } = new();

    public Icon Icons { get; set; } = new();

    public class Icon
    {
        public string ToggleThumbnails { get; set; } = global::MudBlazor.Icons.Material.Filled.ViewSidebar;
        public string PreviousPage { get; set; } = global::MudBlazor.Icons.Material.Filled.ArrowUpward;
        public string NextPage { get; set; } = global::MudBlazor.Icons.Material.Filled.ArrowDownward;
        public string ZoomIn { get; set; } = global::MudBlazor.Icons.Material.Filled.Add;
        public string ZoomOut { get; set; } = global::MudBlazor.Icons.Material.Filled.Remove;
        public string Menu { get; set; } = global::MudBlazor.Icons.Material.Filled.MoreVert;
        public string RotateClockwise { get; set; } = global::MudBlazor.Icons.Material.Filled.RotateRight;
        public string RotateCounterclockwise { get; set; } = global::MudBlazor.Icons.Material.Filled.RotateLeft;
        public string SwitchOrientation { get; set; } = global::MudBlazor.Icons.Material.Filled.ScreenRotation;
        public string FirstPage { get; set; } = global::MudBlazor.Icons.Material.Filled.VerticalAlignTop;
        public string LastPage { get; set; } = global::MudBlazor.Icons.Material.Filled.VerticalAlignBottom;
        public string ResetZoom { get; set; } = global::MudBlazor.Icons.Material.Filled.ResetTv;
        public string PrintDocument { get; set; } = global::MudBlazor.Icons.Material.Filled.Print;
        public string DownloadDocument { get; set; } = global::MudBlazor.Icons.Material.Filled.Download;
    }

    public class Color
    {
        public global::MudBlazor.Color IconColor { get; set; }
        public string BackgroundColor { get; set; } = "#161719";
    }
}