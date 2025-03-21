namespace MudBlazor.PdfViewer.Docs.Code;

public static class Colours
{
    public static string Config =>
        @"builder.Services.AddBlazorPdfViewer(opt =>
{
    // Hot dog stand theme
    opt.Colors.Toolbar = ""#000"";
    opt.Colors.Icon = ""#FF0000"";
    opt.Colors.Background = ""#FFFF00"";
});";
}