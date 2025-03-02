namespace MudBlazorPdf.Pdf;

public class PdfState
{
    public string? Id { get; set; }
    public string? Url { get; set; }

    public int CurrentPage { get; set; }
    public int Orientation { get; set; }
    public double Scale { get; set; }

    public string? Password { get; set; }
}