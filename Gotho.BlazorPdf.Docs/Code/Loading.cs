namespace Gotho.BlazorPdf.Docs.Code;

public static class Loading
{
    public static string FromStream = @"
<label for=""pdfInput"">Select PDF Document:</label>
<InputFile id=""pdfInput"" OnChange=""HandleFileSelected"" accept="".pdf"" class=""form-control"" />

<PdfViewer @ref=""_pdfViewer""/>

@code {
    private long maxFileSize = 1024 * 1024 * 10; // 10 MB limit
    
    public required PdfViewer _pdfViewer { get; set; }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;

        using var stream = file.OpenReadStream(maxFileSize);
        await _pdfViewer.LoadPdfAsync(stream, e.File.Name);
    }
}";

    public static string FromByteArray = @"
<label for=""pdfInput"">Select PDF Document:</label>
<InputFile id=""pdfInput"" OnChange=""HandleFileSelected"" accept="".pdf"" class=""form-control"" />

<PdfViewer @ref=""_pdfViewer""/>

@code {
    private long maxFileSize = 1024 * 1024 * 10; // 10 MB limit
    
    public required PdfViewer _pdfViewer { get; set; }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;

        using var stream = file.OpenReadStream(maxFileSize);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();

        await _pdfViewer.LoadPdfAsync(pdfBytes, e.File.Name);
    }
}";
}