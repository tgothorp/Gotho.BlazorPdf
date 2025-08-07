namespace Gotho.BlazorPdf.Pdf;

public class PdfSearchResult : IEqualityComparer<PdfSearchResult>
{
    public int Page { get; set; }
    public int Index { get; set; }
    public string? Content { get; set; }

    public bool Equals(PdfSearchResult? left, PdfSearchResult? right)
    {
        return left?.Page == right?.Page && left?.Index == right?.Index;
    }

    public int GetHashCode(PdfSearchResult obj)
    {
        return HashCode.Combine(obj.Page, obj.Index);
    }
}