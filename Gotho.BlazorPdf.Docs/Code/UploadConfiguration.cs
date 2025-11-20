namespace Gotho.BlazorPdf.Docs.Code;

public static class UploadConfiguration
{
    public static string EnableUploads => @"
<PdfViewer PermitPdfUploads=""true""/>";

    public static string MaxUploadSize => @"
builder.Services.AddBlazorPdfViewer(config =>
{
    config.MaxPdfFileUploadSize = 15728640; // 15MB
});
";
}