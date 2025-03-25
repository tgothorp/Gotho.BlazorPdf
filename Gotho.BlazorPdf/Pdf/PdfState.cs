namespace Gotho.BlazorPdf.Pdf;

internal class PdfState
{
    public required string? Id { get; set; }
    public required string? Url { get; set; }
    public required string Source { get; set; }

    public required int CurrentPage { get; set; }
    public required int Orientation { get; set; }
    public required double Scale { get; set; }

    public required string? Password { get; set; }

    public required bool Draw { get; set; }
}