namespace MudBlazorPdf;

internal record PdfError
{
    public required PdfErrorType ErrorType { get; init; }
    public required string? Message { get; set; }
}