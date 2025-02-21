namespace MudBlazor.PdfViewer.Docs.Code;

public static class Colours
{
    public static string Config =>
        @"builder.Services.AddMudBlazorPdfViewer(opt =>
{
    // Set the icon colors
    opt.Colors.IconColor = Color.Secondary;
    
    // This can be any valid CSS color value (rgba, hex, etc.)
    opt.Colors.Background = ""#000000"";
});";
}