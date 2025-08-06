namespace Gotho.BlazorPdf.Pdf;

public class Search
{
    public string? SearchQuery { get; set; }
    public bool HighlightAll { get; set; }
    
    public List<PdfSearchResult>? SearchResults { get; set; }
    private PdfSearchResult? _currentSearchResult = null;

    public void UpdateSearchQuery(string searchQuery)
    {
        SearchQuery = searchQuery;
    }

    public void UpdateResults(List<PdfSearchResult> results)
    {
        // Clear search query so we're not scanning text with every update
        SearchQuery = null;
        SearchResults = results;
    }

    public string? GetActiveResult()
    {
        return _currentSearchResult is null 
            ? null 
            : $"{_currentSearchResult.Page},{_currentSearchResult.Index}";
    }
}