namespace MudBlazor.PdfViewer.Docs.Code;

public static class Labels
{
    public static string Config =>
        @"builder.Services.AddBlazorPdfViewer(opt =>
{
    opt.Labels.RotateClockwise = ""Kierrä myötäpäivään"";
    opt.Labels.RotateCounterclockwise = ""Kierrä vastapäivään"";
    opt.Labels.SwitchOrientation = ""Vaihda suunta"";
    opt.Labels.FirstPage = ""Ensimmäinen sivu"";
    opt.Labels.LastPage = ""Viimeinen sivu"";
    opt.Labels.ResetZoom = ""Palauta zoomaus"";
    opt.Labels.PrintDocument = ""Tulosta asiakirja"";
    opt.Labels.DownloadDocument = ""Lataa asiakirja"";
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