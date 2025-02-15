namespace MudBlazor.PdfViewer.Docs;

public static class Examples
{
    public static string BasicExample = @"
<MudPdfViewer
    Class=""pt-5""
    Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""
    OnDocumentLoaded=""OnDocumentLoaded""
    OnPageChanged=""OnPageChanged""/>

<MudText Typo=""Typo.caption"">@eventLog</MudText>

@code {
    private string eventLog { get; set; } = $""Last event: ..., CurrentPage: 0, TotalPages: 0"";

    private void OnDocumentLoaded(PdfViewerEventArgs args)
        => eventLog = $""Last event: OnDocumentLoaded, CurrentPage: {args.CurrentPage}, TotalPages: {args.TotalPages}"";

    private void OnPageChanged(PdfViewerEventArgs args)
        => eventLog = $""Last event: OnPageChanged, CurrentPage: {args.CurrentPage}, TotalPages: {args.TotalPages}"";
}
";

    public static string Thumbnails = @"
<MudPdfViewer
    Class=""pt-5""
    HideThumbnails=""true""
    Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""/>
";
    
    public static string Orientation = @"
<MudPdfViewer
    Class=""pt-5""
    HideThumbnails=""true""
    Orientation=""Orientation.Landscape""
    Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""/>";

    public static string Configuration = @"
builder.Services.AddMudBlazorPdfViewer(opt =>
{
    // Change zoom in / out icons
    opt.Icons.ZoomIn = Icons.Material.Filled.SmokeFree;
    opt.Icons.ZoomOut = Icons.Material.Filled.SmokeFree;
    
    // Change menu item text
    opt.Labels.RotateClockwise = ""Spin it to the right"";
    opt.Labels.RotateCounterclockwise = ""Spin it to the left"";

    // Change color of icons and background
    opt.Colors.IconColor = MudBlazor.Color.Secondary;
    opt.Colors.Background = ""#161719"";
});";
}