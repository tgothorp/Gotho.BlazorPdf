namespace Gotho.BlazorPdf.Pdf;

internal record Page
{
    public int CurrentPage { get; private set; } = 0;
    public int TotalPages { get; private set; } = 0;

    public void Update(int currentPage, int totalPages)
    {
        CurrentPage = currentPage;
        TotalPages = totalPages;
    }

    public bool NextPage()
    {
        if (CurrentPage == TotalPages)
            return false;

        CurrentPage++;
        return true;
    }

    public bool PreviousPage()
    {
        if (CurrentPage == 1)
            return false;

        CurrentPage--;
        return true;
    }

    public bool FirstPage()
    {
        if (CurrentPage == 1)
            return false;

        CurrentPage = 1;
        return true;
    }

    public bool LastPage()
    {
        if (CurrentPage == TotalPages)
            return false;

        CurrentPage = TotalPages;
        return true;
    }

    public bool GotoPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > TotalPages)
            return false;

        CurrentPage = pageNumber;
        return true;
    }
}