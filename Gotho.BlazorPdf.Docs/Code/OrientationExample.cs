namespace Gotho.BlazorPdf.Docs.Code;

public static class OrientationExample
{
    public static string Example =>
@"<PdfViewer 
  PdfOrientation=""PdfOrientation.Landscape""
  Url=""https://raw.githubusercontent.com/tgothorp/MudBlazor.PdfViewer/refs/heads/main/files/test_pdf_document.pdf""/>";
}