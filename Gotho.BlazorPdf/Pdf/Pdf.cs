using Gotho.BlazorPdf.Extensions;

namespace Gotho.BlazorPdf.Pdf;

internal class Pdf
{
    public Pdf(string id, string? url, PdfOrientation orientation)
    {
        Id = id;
        Orientation = new Orientation(orientation);
        UpdateUrl(url);
    }

    public string Id { get; init; }
    public string? Url { get; private set; }
    public PdfSource Source { get; private set; }

    public Orientation Orientation { get; init; }
    public Zoom Zooming { get; init; } = new();
    public Page Paging { get; init; } = new();

    public string? Password { get; private set; } = null;

    public void UpdateUrl(string? url)
    {
        Url = url;

        if (string.IsNullOrWhiteSpace(url))
            Source = PdfSource.Base64;
        else
            Source = Url.IsProbablyUrl()
                ? PdfSource.Url
                : Url.IsProbablyBase64()
                    ? PdfSource.Base64
                    : PdfSource.Binary;
    }

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
            Source = Source.ToString(),
            CurrentPage = Paging.CurrentPage,
            Orientation = Orientation.GetOrientation(),
            Scale = Zooming.GetScale(),
            Password = Password,
        };
    }
}