using Gotho.BlazorPdf.Extensions;

namespace Gotho.BlazorPdf.Pdf;

/// <summary>
/// An internal representation of a PDF file used by BlazorPDF to manage PDF state
/// </summary>
/// <remarks>
/// You should not need to create an instance of this class
/// </remarks>
public class Pdf
{
    public Pdf(string url, string? fileName, PdfOrientation orientation, bool scrollMode)
    {
        Id = "".GenerateRandomString();
        ScrollMode = scrollMode;
        Orientation = new Orientation(orientation);

        if (url.IsProbablyBase64())
        {
            FileBytes = ConvertBase64ToByte(url);
            FileName = fileName;
        }
        else
        {
            Url = url;
            FileName = null;
        }
    }

    public Pdf(byte[] fileBytes, string fileName, PdfOrientation orientation, bool scrollMode)
    {
        Id = "".GenerateRandomString();
        Url = null;
        FileBytes = fileBytes;
        FileName = fileName;
        ScrollMode = scrollMode;
        Orientation = new Orientation(orientation);
    }

    public string Id { get; init; }
    public string? Url { get; private set; }
    public string? FileName { get; private set; }
    public byte[]? FileBytes { get; private set; }
    public bool ScrollMode { get; private set; }

    public Orientation Orientation { get; init; }
    public Zoom Zooming { get; init; } = new();
    public Page Paging { get; init; } = new();
    public DrawLayer DrawLayer { get; set; } = new();
    public Search Search { get; set; } = new();

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
            FileName = FileName,
            FileBytes = FileBytes,
            CurrentPage = Paging.CurrentPage,
            Orientation = Orientation.GetOrientation(),
            ScrollMode = ScrollMode,
            Scale = Zooming.GetScale(),
            Password = Password,
            DrawLayerEnabled = DrawLayer.Enabled,
            PenColor = DrawLayer.PenColor,
            PenThickness = DrawLayer.PenThickness,
            SearchQuery = Search.SearchQuery,
            ActiveResultIndex = Search.GetSearchIndex(Paging.CurrentPage)
        };
    }

    private byte[] ConvertBase64ToByte(string base64)
    {
        if (base64.Contains(","))
        {
            base64 = base64.Split(',')[1];
        }

        return Convert.FromBase64String(base64);
    }
}