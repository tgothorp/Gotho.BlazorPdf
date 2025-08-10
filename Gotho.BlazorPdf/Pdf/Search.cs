namespace Gotho.BlazorPdf.Pdf;

public class Search
{
    public string? SearchQuery { get; private set; }
    public PdfSearchResult? CurrentSearchResult { get; private set; }
    public List<PdfSearchResult>? SearchResults { get; set; }

    public void UpdateSearchQuery(string? searchQuery)
    {
        SearchQuery = searchQuery;
        SearchResults = null;
        CurrentSearchResult = null;
    }

    public void UpdateResults(List<PdfSearchResult> results)
    {
        // Clear search query so we're not scanning text with every update
        SearchQuery = null;
        SearchResults = results;
        CurrentSearchResult = SearchResults.FirstOrDefault();
    }

    public bool NextResult()
    {
        if (SearchResults is null)
            return false;

        if (CurrentSearchResult == null)
        {
            CurrentSearchResult = SearchResults.FirstOrDefault();
            return true;
        }

        if (SearchResults.Last().Equals(CurrentSearchResult!))
        {
            CurrentSearchResult = SearchResults.First();
            return true;
        }

        var index = SearchResults.IndexOf(CurrentSearchResult!);
        CurrentSearchResult = SearchResults[index + 1];
        return true;
    }

    public bool PreviousResult()
    {
        if (SearchResults is null)
            return false;

        if (CurrentSearchResult == null)
        {
            CurrentSearchResult = SearchResults.FirstOrDefault();
            return true;
        }

        if (SearchResults.First().Equals(CurrentSearchResult!))
        {
            CurrentSearchResult = SearchResults.Last();
            return true;
        }

        var index = SearchResults.IndexOf(CurrentSearchResult!);
        CurrentSearchResult = SearchResults[index - 1];
        return true;
    }

    public int GetDisplayIndex()
    {
        if (SearchResults is null || SearchResults.Count == 0 || CurrentSearchResult is null)
            return 0; 
        
        var index = SearchResults.IndexOf(CurrentSearchResult!);
        return index + 1;
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