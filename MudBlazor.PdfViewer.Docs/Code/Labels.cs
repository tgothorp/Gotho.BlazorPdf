namespace MudBlazor.PdfViewer.Docs.Code;

public static class Labels
{
    public static string Config =>
        @"builder.Services.AddMudBlazorPdfViewer(opt =>
{
    opt.Labels.PrintDocument = ""Imprimir Documento"";
});";

    public static string All =>
        @"public string ToggleThumbnails { get; set; } = ""Show Thumbnails"";
public string PreviousPage { get; set; } = ""Previous Page"";
public string NextPage { get; set; } = ""Next Page"";
public string PageOf { get; set; } = ""of"";
public string ZoomIn { get; set; } = ""Zoom In"";
public string ZoomOut { get; set; } = ""Zoom Out"";
public string OpenMenu { get; set; } = ""Menu"";
public string RotateClockwise { get; set; } = ""Rotate Clockwise"";
public string RotateCounterclockwise { get; set; } = ""Rotate Counterclockwise"";
public string SwitchOrientation { get; set; } = ""Switch Orientation"";
public string FirstPage { get; set; } = ""First Page"";
public string LastPage { get; set; } = ""Last Page"";
public string ResetZoom { get; set; } = ""Reset Zoom"";
public string PrintDocument { get; set; } = ""Print Document"";";
}