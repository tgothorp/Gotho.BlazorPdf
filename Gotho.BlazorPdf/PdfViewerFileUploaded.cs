namespace Gotho.BlazorPdf;

public class PdfViewerFileUploaded
{
    public required string FileName { get; set; }
    public required byte[] Contents { get; set; }
    public required long Size { get; set; }
}