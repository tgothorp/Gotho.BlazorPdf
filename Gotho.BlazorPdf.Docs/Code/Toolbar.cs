namespace Gotho.BlazorPdf.Docs.Code;

public static class Toolbar
{
    public static string Minimal =>
@"<PdfViewer 
    Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf"" 
    HideDropdownMenu=""true"" 
    PrintButtonLocation=""PdfMenuItemLocation.Toolbar"" 
    FindButtonLocation=""PdfMenuItemLocation.None"" 
    DownloadButtonLocation=""PdfMenuItemLocation.Toolbar""/>";
}