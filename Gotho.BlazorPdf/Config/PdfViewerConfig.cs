namespace Gotho.BlazorPdf.Config;

public class PdfViewerConfig
{
    /// <summary>
    /// Indicates whether the PDF.js worker script should be loaded from a local project path using a Blob URL,
    /// instead of the default method used by the browser.
    /// </summary>
    /// <remarks>
    /// This setting exists to support platforms where standard Web Workers cannot be loaded using script URLs,
    /// such as in .NET MAUI Blazor Hybrid applications.
    /// 
    /// <para><b>Default:</b> <c>false</c></para>
    /// </remarks>
    public bool UseProjectWorker { get; set; } = false;
}