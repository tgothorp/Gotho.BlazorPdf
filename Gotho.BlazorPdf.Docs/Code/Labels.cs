namespace Gotho.BlazorPdf.Docs.Code;

public static class Labels
{
    public static string Config =>
@"
<PdfViewer 
    LocalizedStrings=""FinnishStrings"" 
    Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""/>

@code {
    private BlazorPdfLocalizedStrings FinnishStrings = new()
    { 
        ToggleThumbnails = ""Näytä esikatselukuvat"",
        PreviousPage = ""Edellinen sivu"",
        NextPage = ""Seuraava sivu"",
        PageOf = ""/"",
        ZoomIn = ""Lähennä"",
        ZoomOut = ""Loitonna"",
        RotateClockwise = ""Kierrä myötäpäivään"",
        RotateCounterclockwise = ""Kierrä vastapäivään"",
        SwitchOrientation = ""Vaihda suuntaa"",
        FirstPage = ""Ensimmäinen sivu"",
        LastPage = ""Viimeinen sivu"",
        ResetZoom = ""Palauta zoomaus"",
        PrintDocument = ""Tulosta asiakirja"",
        DownloadDocument = ""Lataa asiakirja"",
        Draw = ""Piirrä"",
        DrawingTools = ""Piirtotyökalut"",
        DrawingColor = ""Väri"",
        DrawingThickness = ""Viivan paksuus"",
        DrawingUndo = ""Kumoa"",
        DrawingClear = ""Tyhjennä""
    };
}
";

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