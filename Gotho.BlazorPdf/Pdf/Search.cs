namespace Gotho.BlazorPdf.Pdf;

public class Search
{
    public string? SearchQuery { get; set; }
    public List<PdfSearchResult>? SearchResults { get; set; }

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
}