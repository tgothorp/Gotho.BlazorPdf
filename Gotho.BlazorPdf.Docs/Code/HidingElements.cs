namespace Gotho.BlazorPdf.Docs.Code;

public static class HidingElements
{
    public static string Config =>
@"
@* Fully hide the menu *@
<PdfViewer HideThumbnails=""false""
		   SinglePageMode=""false""
		   Height=""100vh""
		   Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""
		   HideMenu=""true"" />

@* Or hide the download or print buttons *@
<PdfViewer HideThumbnails=""false""
		   SinglePageMode=""false""
		   Height=""100vh""
		   Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""
		   HideDownloadButton=""true""
		   HidePrintButton=""true"" />
";
}