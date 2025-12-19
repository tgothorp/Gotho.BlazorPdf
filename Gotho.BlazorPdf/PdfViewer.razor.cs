using System.Text.Json;
using Gotho.BlazorPdf.Config;
using Gotho.BlazorPdf.Extensions;
using Gotho.BlazorPdf.Pdf;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Gotho.BlazorPdf;

public partial class PdfViewer : ComponentBase
{
    protected bool Loading = true;
    protected DotNetObjectReference<PdfViewer>? ObjectReference;
    protected PdfError? Error;
    protected string? PdfPassword;
    protected string? PdfUploadError;
    protected PdfMetadata? Metadata;

    public Pdf.Pdf? PdfFile { get; set; }

    /// <summary>
    /// Sets the display orientation of the PDF document
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Orientation.Portrait</c>
    /// </remarks>
    [Parameter]
    public PdfOrientation PdfOrientation { get; set; } = PdfOrientation.Portrait;

    /// <summary>
    /// Determines the height of the PDF viewer when in scrolling mode.
    /// This can be any valid CSS height value (250px, 100vh, 85% etc.)
    /// </summary>
    /// <remarks>
    /// Defaults to <c>65vh</c>
    /// </remarks>
    [Parameter]
    public string Height { get; set; } = "65vh";

    /// <summary>
    /// If this is set to true then the pages of the PDF document will be displayed one at a time,
    /// if this is set to false then all pages are shown at once with the ability to scroll between them.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>
    /// </remarks>
    [Parameter]
    public bool ScrollMode { get; set; } = false;

    /// <summary>
    /// URL of the PDF to be displayed, this can also be a base64 string 
    /// </summary>
    [Parameter]
    public string? Url { get; set; }

    /// <summary>
    /// Hides the thumbnail bar as well as the option to display it
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    [Parameter]
    public bool HideThumbnails { get; set; }

    /// <summary>
    /// If no <c>URL</c> parameter is specified in the PdfViewer component then a user will be allowed to upload
    /// a PDF file unless option is set to <c>false</c>
    /// </summary>
    /// <remarks>
    /// As always, you should consider any potential security implications of allowing users to upload their own files
    ///
    /// <para><b>Default:</b> <c>false</c></para>
    /// </remarks>
    [Parameter]
    public bool PermitPdfUploads { get; set; } = false;

    /// <summary>
    /// This event fires immediately after the PDF document is loaded.
    /// </summary>
    [Parameter]
    public EventCallback<PdfViewerEventArgs> OnDocumentLoaded { get; set; }

    /// <summary>
    /// This event fires immediately after the page is changed.
    /// </summary>
    [Parameter]
    public EventCallback<PdfViewerEventArgs> OnPageChanged { get; set; }

    /// <summary>
    /// Invoked when a user uploads a file
    /// </summary>
    [Parameter]
    public EventCallback<PdfViewerFileUploaded> OnFileUploaded { get; set; }

    /// <summary>
    /// A class containing the localized strings for the viewer 
    /// </summary>
    [Parameter]
    public BlazorPdfLocalizedStrings LocalizedStrings { get; set; } = new();

    /// <summary>
    /// A class containing the colors for the PDF viewer
    /// </summary>
    [Parameter]
    public BlazorPdfColors Colors { get; set; } = new();

    /// <summary>
    /// Hides the dropdown menu. (default: false)
    /// </summary>
    [Parameter]
    public bool HideDropdownMenu { get; set; } = false;

    /// <summary>
    /// Should the option to print the PDF document be displayed on the toolbar, in the dropdown menu, in both, or not at all? (default: Menu)
    /// </summary>
    [Parameter]
    public PdfMenuItemLocation PrintButtonLocation { get; set; } = PdfMenuItemLocation.Menu;

    /// <summary>
    /// Should the option to download the PDF document be displayed on the toolbar, in the dropdown menu, in both, or not at all? (default: Menu)
    /// </summary>
    [Parameter]
    public PdfMenuItemLocation DownloadButtonLocation { get; set; } = PdfMenuItemLocation.Menu;

    /// <summary>
    /// Should the option to find text in the PDF document be displayed on the toolbar, in the dropdown menu, in both, or not at all? (default: Toolbar)
    /// </summary>
    /// <remarks>
    /// The option to find text in the PDF document is ALWAYS disabled for scroll mode
    /// </remarks>
    [Parameter]
    public PdfMenuItemLocation FindButtonLocation { get; set; } = PdfMenuItemLocation.Toolbar;

    /// <summary>
    /// Should the option to draw on the PDF document be displayed on the toolbar, in the dropdown menu, in both, or not at all? (default: Menu)
    /// </summary>
    /// <remarks>
    /// The option to draw on the PDF document is ALWAYS disabled for scroll mode
    /// </remarks>
    [Parameter]
    public PdfMenuItemLocation DrawButtonLocation { get; set; } = PdfMenuItemLocation.Menu;

    [Inject] private PdfInterop PdfInterop { get; set; } = default!;
    [Inject] protected BlazorPdfConfig Config { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ObjectReference ??= DotNetObjectReference.Create(this);

        if (!Url.IsNullOrEmpty())
            PdfFile = new Pdf.Pdf(Url!, "Pdf Document", Url.IsProbablyUrl() ? PdfSource.Url : PdfSource.Base64, PdfOrientation, ScrollMode);
        else
            Loading = false;

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && PdfFile is not null)
            await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, Config.UseProjectWorker);

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// Invoked by BlazorPdf's JS interop code when a PDF file has been fully loaded
    /// </summary>
    /// <remarks>Do not call this method from your code</remarks>
    [JSInvokable]
    public void DocumentLoaded(PdfViewerModel? pdfViewerModel)
    {
        Loading = false;

        if (pdfViewerModel is null)
            return;

        PdfFile!.Paging.Update(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages);
        StateHasChanged();

        if (OnDocumentLoaded.HasDelegate)
            OnDocumentLoaded.InvokeAsync(new PdfViewerEventArgs(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages));
    }

    /// <summary>
    /// Invoked by BlazorPdf's JS interop code when a PDF's state has changed, usually when a user has changed page 
    /// </summary>
    /// <remarks>Do not call this method from your code</remarks>
    [JSInvokable]
    public void SetPdfViewerMetaData(PdfViewerModel? pdfViewerModel)
    {
        if (pdfViewerModel is null)
            return;

        PdfFile!.Paging.Update(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages);
        StateHasChanged();

        if (OnPageChanged.HasDelegate)
            OnPageChanged.InvokeAsync(new PdfViewerEventArgs(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages));
    }

    [JSInvokable]
    public void PdfMetadata(PdfMetadata metadata)
    {
        Metadata = metadata;
        StateHasChanged();
    }

    /// <summary>
    /// Invoked by BlazorPdf's JS interop code when a PDF file fails to load, usually due to requiring a password
    /// </summary>
    /// <remarks>Do not call this method from your code</remarks>
    [JSInvokable]
    public void PdfViewerError(PdfViewerError error)
    {
        Loading = false;
        Error = error.Name switch
        {
            "PasswordException" => new PdfError { ErrorType = PdfErrorType.PasswordRequired, Message = error.Message?.ToLower() == "no password given" ? null : error.Message },
            _ => new PdfError { ErrorType = PdfErrorType.Error, Message = error.Message }
        };

        StateHasChanged();
    }

    /// <summary>
    /// Invoked by BlazorPdf's JS interops code when text searching
    /// </summary>
    /// <remarks>Do not call this method from your code</remarks>
    [JSInvokable]
    public async Task SearchResultsFromStream(IJSStreamReference stream)
    {
        await using var s = await stream.OpenReadStreamAsync(maxAllowedSize: 50 * 1024 * 1024);
        using var reader = new StreamReader(s);
        var json = await reader.ReadToEndAsync();
        var results = JsonSerializer.Deserialize<List<PdfSearchResult>>(json);

        PdfFile?.Search.UpdateResults(results ?? []);

        if (PdfFile?.Search.CurrentSearchResult is not null
            && PdfFile?.Search.CurrentSearchResult?.Page != PdfFile?.Paging.CurrentPage)
            PdfFile?.Paging.GotoPage(PdfFile.Search.CurrentSearchResult!.Page);

        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    #region Loading

    public async Task LoadPdfAsync(string? urlOrBase64String, string? fileName = "PDF Document")
    {
        ArgumentNullException.ThrowIfNull(urlOrBase64String);

        PdfFile = new Pdf.Pdf(urlOrBase64String, fileName, urlOrBase64String.IsProbablyUrl() ? PdfSource.Url : PdfSource.Base64, PdfOrientation, ScrollMode);
        Loading = true;
        Error = null;
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, Config.UseProjectWorker);
    }

    public async Task LoadPdfAsync(Stream stream, string? fileName = "PDF Document")
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        await LoadPdfAsync(ms.ToArray(), fileName);
    }

    public async Task LoadPdfAsync(byte[] pdfBytes, string? fileName = "PDF Document")
    {
        PdfFile = new Pdf.Pdf(pdfBytes, fileName!, PdfOrientation, ScrollMode);

        Loading = true;
        Error = null;
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, Config.UseProjectWorker);
    }

    #endregion

    #region Paging

    internal async Task FirstPageAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Paging.FirstPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task LastPageAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Paging.LastPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task NextPageAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Paging.NextPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task PreviousPageAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Paging.PreviousPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task PageNumberChanged(int value)
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Paging.GotoPage(value))
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Zooming

    internal async Task ZoomInAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Zooming.ZoomIn())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task ZoomOutAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Zooming.ZoomOut())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task ResetZoomAsync()
    {
        if (PdfFile is null)
            return;

        if (PdfFile.Zooming.ResetZoom())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Rotation

    internal async Task RotateClockwiseAsync()
    {
        if (PdfFile is null)
            return;

        PdfFile.Orientation.RotateClockwise();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task RotateCounterclockwiseAsync()
    {
        if (PdfFile is null)
            return;

        PdfFile.Orientation.RotateCounterClockwise();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task SwitchOrientationAsync()
    {
        if (PdfFile is null)
            return;

        PdfFile.Orientation.Flip();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Drawing

    internal async Task ToggleDrawingAsync()
    {
        if (PdfFile is null)
            return;

        PdfFile.DrawLayer.Toggle();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task UpdatePenColorAsync(string color)
    {
        if (PdfFile is null)
            return;

        PdfFile.DrawLayer.UpdateColor(color);
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task UpdatePenThickness(int thickness)
    {
        if (PdfFile is null)
            return;

        PdfFile.DrawLayer.UpdateThickness(thickness);
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    internal async Task UndoLastStrokeAsync()
    {
        if (PdfFile is null)
            return;

        await PdfInterop.UndoLastStrokeAsync(ObjectReference!, PdfFile);
    }

    internal async Task ClearAllPageStrokesAsync()
    {
        if (PdfFile is null)
            return;

        await PdfInterop.ClearStrokesForPageAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Searching

    internal async Task Search(string query)
    {
        PdfFile?.Search.UpdateSearchQuery(query);
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    internal async Task ClearSearchResults()
    {
        PdfFile?.Search.UpdateSearchQuery(null);
        await PdfInterop.ClearSearchResults(ObjectReference!, PdfFile!);
    }

    internal async Task NextResult()
    {
        if (!PdfFile!.Search.NextResult())
        {
            return;
        }

        if (PdfFile?.Search.CurrentSearchResult?.Page != PdfFile?.Paging.CurrentPage)
            PdfFile?.Paging.GotoPage(PdfFile.Search.CurrentSearchResult!.Page);

        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    internal async Task PreviousResult()
    {
        if (!PdfFile!.Search.PreviousResult())
        {
            return;
        }

        if (PdfFile?.Search.CurrentSearchResult?.Page != PdfFile?.Paging.CurrentPage)
            PdfFile?.Paging.GotoPage(PdfFile.Search.CurrentSearchResult!.Page);

        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    #endregion

    #region Other

    internal async Task DownloadDocumentAsync()
    {
        await PdfInterop.DownloadDocumentAsync(ObjectReference!, PdfFile!);
    }

    internal async Task PrintDocumentAsync()
    {
        await PdfInterop.PrintDocumentAsync(ObjectReference!, PdfFile!);
    }

    internal async Task ViewMetadataAsync()
    {
        await PdfInterop.ViewMetadataAsync(ObjectReference!, PdfFile!);
    }

    internal void ClearMetadata()
    {
        Metadata = null;
        StateHasChanged();
    }

    protected async Task UploadFile(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file.Size > Config.MaxPdfFileUploadSize)
        {
            PdfUploadError = LocalizedStrings.UploadTooLarge;
            return;
        }

        if (file.ContentType != "application/pdf")
        {
            PdfUploadError = LocalizedStrings.UploadWrongFormat;
            return;
        }

        await using var stream = file.OpenReadStream(Config.MaxPdfFileUploadSize);
        await LoadPdfAsync(stream, file.Name);
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile!, Config.UseProjectWorker);

        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        await OnFileUploaded.InvokeAsync(new PdfViewerFileUploaded
        {
            FileName = file.Name,
            Size = file.Size,
            Contents = ms.ToArray()
        });
    }

    protected async Task UpdatePdfPassword()
    {
        if (string.IsNullOrEmpty(PdfPassword))
        {
            Error = new PdfError
            {
                ErrorType = PdfErrorType.PasswordRequired,
                Message = "Please supply a password."
            };
            StateHasChanged();
            return;
        }

        PdfFile!.UpdatePassword(PdfPassword);

        Loading = true;
        Error = null;
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, Config.UseProjectWorker);
    }

    #endregion
}