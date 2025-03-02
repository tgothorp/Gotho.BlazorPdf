using Gotho.BlazorPdf;
using MudBlazorPdf;

public class MudPdfViewerConfig : PdfViewerConfig
{
    public MudBlazor.Color IconColor { get; set; } = MudBlazor.Color.Default;
    public Icon Icons { get; set; } = new();

    public class Icon
    {
        public string ToggleThumbnails { get; set; } = MudBlazor.Icons.Material.Filled.ViewSidebar;
        public string PreviousPage { get; set; } = MudBlazor.Icons.Material.Filled.ArrowUpward;
        public string NextPage { get; set; } = MudBlazor.Icons.Material.Filled.ArrowDownward;
        public string ZoomIn { get; set; } = MudBlazor.Icons.Material.Filled.Add;
        public string ZoomOut { get; set; } = MudBlazor.Icons.Material.Filled.Remove;
        public string Menu { get; set; } = MudBlazor.Icons.Material.Filled.MoreVert;
        public string RotateClockwise { get; set; } = MudBlazor.Icons.Material.Filled.RotateRight;
        public string RotateCounterclockwise { get; set; } = MudBlazor.Icons.Material.Filled.RotateLeft;
        public string SwitchOrientation { get; set; } = MudBlazor.Icons.Material.Filled.ScreenRotation;
        public string FirstPage { get; set; } = MudBlazor.Icons.Material.Filled.VerticalAlignTop;
        public string LastPage { get; set; } = MudBlazor.Icons.Material.Filled.VerticalAlignBottom;
        public string ResetZoom { get; set; } = MudBlazor.Icons.Material.Filled.ResetTv;
        public string PrintDocument { get; set; } = MudBlazor.Icons.Material.Filled.Print;
        public string DownloadDocument { get; set; } = MudBlazor.Icons.Material.Filled.Download;
    }
}