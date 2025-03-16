namespace Gotho.BlazorPdf;

public class PdfViewerEventArgs(int currentPage, int totalPages)
{
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int CurrentPage { get; } = currentPage;

    /// <summary>
    /// Gets the total pages count.
    /// </summary>
    public int TotalPages { get; } = totalPages;
}