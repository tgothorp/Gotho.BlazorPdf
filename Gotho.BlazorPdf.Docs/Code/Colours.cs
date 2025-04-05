namespace Gotho.BlazorPdf.Docs.Code;

public static class Colours
{
    public static string Config =>
@"
<PdfViewer
    Colors=""PdfColors""
    Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""/>

@code {

    private BlazorPdfColors PdfColors { get; set; } = new()
    {
        Background = ""#FF0000"",
        Toolbar = ""#00FF00"",
        Icon = ""#0000FF"",
    };
}";
}