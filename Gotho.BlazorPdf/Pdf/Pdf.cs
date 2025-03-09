namespace Gotho.BlazorPdf.Pdf;

public class Pdf(string id, string url, PdfOrientation orientation)
{
    public string Id { get; init; } = id;
    public string Url { get; init; } = url;

    public Orientation Orientation { get; init; } = new(orientation);
    public Zoom Zooming { get; init; } = new();
    public Page Paging { get; init; } = new();

    public string? Password { get; private set; } = null;

    public void UpdatePassword(string? password)
    {
        Password = password;
    }

    internal PdfState GetPdfState()
    {
        return new PdfState
        {
            Id = Id,
            Url = Url,
            CurrentPage = Paging.CurrentPage,
            Orientation = Orientation.GetOrientation(),
            Scale = Zooming.GetScale(),
            Password = Password,
        };
    }
}