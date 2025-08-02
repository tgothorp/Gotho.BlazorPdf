namespace Gotho.BlazorPdf.Config;

public class BlazorPdfConfig
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

    /// <summary>
    /// If no <c>URL</c> parameter is specified in the PdfViewer component then a user will be allowed to upload
    /// a PDF file unless option is set to <c>false</c>
    /// </summary>
    /// <remarks>
    /// As always, you should consider any potential security implications of allowing users to upload their own files
    ///
    /// <para><b>Default:</b> <c>true</c></para>
    /// </remarks>
    public bool PermitPdfFileUpload { get; set; } = true;

    /// <summary>
    /// Determines the maximum file size that can be uploaded, used in conjunction with <c>PermitPdfFileUpload</c>
    /// </summary>
    /// <remarks>
    /// <para><b>Default:</b> 10MB (<c>10 * 1024 * 1024</c>)</para>
    /// </remarks>
    public long MaxPdfFileUploadSize { get; set; } = 10 * 1024 * 1024;
}