namespace Gotho.BlazorPdf.MudBlazor.Config;

public class MudPdfViewerConfig
{
    public Color Colors { get; set; } = new();

    public class Color
    {
        public global::MudBlazor.Color IconColor { get; set; }
        public string BackgroundColor { get; set; } = "#161719";
    }
}