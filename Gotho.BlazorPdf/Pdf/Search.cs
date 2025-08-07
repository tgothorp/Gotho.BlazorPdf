namespace Gotho.BlazorPdf.Pdf;

public class Search
{
    public string? SearchQuery { get; private set; }
    public PdfSearchResult? CurrentSearchResult { get; private set; }
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
        CurrentSearchResult = SearchResults.FirstOrDefault();
    }

    public void NextResult()
    {
        if (SearchResults is null)
            return;

        if (CurrentSearchResult == null)
        {
            CurrentSearchResult = SearchResults.FirstOrDefault();
            return;
        }

        if (SearchResults.Last().Equals(CurrentSearchResult!))
        {
            CurrentSearchResult = SearchResults.First();
            return;
        }

        var index = SearchResults.IndexOf(CurrentSearchResult!);
        CurrentSearchResult = SearchResults[index + 1];
    }

    public void PreviousResult()
    {
        if (SearchResults is null)
            return;

        if (CurrentSearchResult == null)
        {
            CurrentSearchResult = SearchResults.FirstOrDefault();
            return;
        }

        if (SearchResults.First().Equals(CurrentSearchResult!))
        {
            CurrentSearchResult = SearchResults.Last();
            return;
        }

        var index = SearchResults.IndexOf(CurrentSearchResult!);
        CurrentSearchResult = SearchResults[index - 1];
    }

    public int? GetSearchIndex(int currentPage)
    {
        if (SearchResults is null || SearchResults.Count == 0 || CurrentSearchResult is null)
            return null;

        var resultsByPage = SearchResults
            .Where(x => x.Page == currentPage)
            .ToList();
        
        return resultsByPage.IndexOf(CurrentSearchResult!);
    }
}