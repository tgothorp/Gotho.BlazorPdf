namespace MudBlazor.PdfViewer.Docs.Code;

public static class Events
{
    public static string Example =>
@"<MudText Class=""my-2"">@eventLog</MudText>

<PdfViewer
  OnDocumentLoaded=""OnDocumentLoaded""
  OnPageChanged=""OnPageChanged""
  Url=""https://raw.githubusercontent.com/tgothorp/MudBlazor.PdfViewer/refs/heads/main/files/test_pdf_document.pdf""/>

@code {
    private string eventLog { get; set; } = $""Last event: ..., CurrentPage: 0, TotalPages: 0"";

    private void OnDocumentLoaded(PdfViewerEventArgs args)
        => eventLog = $""Last event: OnDocumentLoaded, CurrentPage: {args.CurrentPage}, TotalPages: {args.TotalPages}"";

    private void OnPageChanged(PdfViewerEventArgs args)
        => eventLog = $""Last event: OnPageChanged, CurrentPage: {args.CurrentPage}, TotalPages: {args.TotalPages}"";
}";
}