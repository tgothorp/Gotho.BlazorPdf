namespace Gotho.BlazorPdf.Docs.Code;

public static class UploadConfiguration
{
    public static string EnableUploads => @"
<PdfViewer PermitPdfUploads=""true""/>";

    public static string MaxUploadSize => @"
builder.Services.AddBlazorPdfViewer(config =>
{
    config.MaxPdfFileUploadSize = 15728640; // 15MB
});";

    public static string CallbackObject => @"
public class PdfViewerFileUploaded
{
  public required string FileName { get; set; }

  public required byte[] Contents { get; set; }

  public required long Size { get; set; }
}";
    
    public static string OnUpload => @"
<PdfViewer PermitPdfUploads=""true"" OnFileUploaded=@FileUploaded/>

@code
{
    [Inject] public ISnackbar Snackbar { get; set; }
    
    private void FileUploaded(PdfViewerFileUploaded data)
    { 
        var fileInfo = $""Uploaded {data.FileName} ({data.Size} bytes)"";
        Snackbar.Add(new MarkupString(fileInfo));
    }
}";
}