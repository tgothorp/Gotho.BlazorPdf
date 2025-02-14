namespace MudBlazorPdf;

public class MudPdfViewerConfig
{
    public Label Labels { get; set; } = new();
    public Icon Icons { get; set; } = new();
    
    public class Label
    {
        public string ToddleThumbnails { get; set; } = "Show Thumbnails";
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
    }
    
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
    }
}