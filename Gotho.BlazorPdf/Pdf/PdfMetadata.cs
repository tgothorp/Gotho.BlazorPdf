namespace Gotho.BlazorPdf.Pdf;

public class PdfMetadata
{
    public string? Author { get; set; }
    public string? Creator { get; set; }
    public string? Keywords { get; set; }
    public string? Producer { get; set; }
    public string? Subject { get; set; }
    public string? Title { get; set; }
    public string? FormatVersion { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    public Dictionary<string, string>? CustomMetadata { get; set; }
}