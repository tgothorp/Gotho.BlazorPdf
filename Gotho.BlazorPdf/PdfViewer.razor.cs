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
    protected PdfMetadata? Metadata;

    protected Pdf.Pdf? PdfFile { get; set; }

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
    public bool SinglePageMode { get; set; } = true;

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
    /// Invoked when a file is uploaded by a user
    /// </summary>
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

    [Inject] private PdfInterop PdfInterop { get; set; } = default!;
    [Inject] protected BlazorPdfConfig Config { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ObjectReference ??= DotNetObjectReference.Create(this);
        
        if (!Url.IsNullOrEmpty())
            PdfFile = new Pdf.Pdf("".GenerateRandomString(), Url!, PdfOrientation);
        else
            Loading = false;

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && PdfFile is not null)
            await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, SinglePageMode, Config.UseProjectWorker);

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
    /// Invoked by BlazorPdf's JS interop code when viewing a PDF's metadata
    /// </summary>
    /// <remarks>Do not call this method from your code</remarks>
    [JSInvokable]
    public void PdfMetadata(PdfMetadata metadata)
    {
        Metadata = metadata;
        StateHasChanged();
    }

    /// <summary>
    /// Invoked by BlazorPdf's JS interops code when text searching
    /// </summary>
    /// <remarks>Do not call this method from your code</remarks>
    [JSInvokable]
    public async Task SearchResults(List<PdfSearchResult> results)
    {
        PdfFile?.Search.UpdateResults(results);
        
        if (PdfFile?.Search.CurrentSearchResult?.Page != PdfFile?.Paging.CurrentPage)
            PdfFile?.Paging.GotoPage(PdfFile.Search.CurrentSearchResult!.Page);

        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    /// <summary>
    /// Loads a PDF from the given URL, can be used as an alternative to the <c>Url</c> parameter.
    /// </summary>
    /// <param name="url">This can be a URL or a Base64 string</param>
    public async Task LoadPdfAsync(string? url = null)
    {
        if (Error is not null && Error.ErrorType == PdfErrorType.PasswordRequired && string.IsNullOrEmpty(PdfPassword))
        {
            Error.Message = "Please supply a password.";
            StateHasChanged();
            return;
        }
        
        if (PdfFile is null)
            PdfFile = new Pdf.Pdf("".GenerateRandomString(), url!, PdfOrientation);
        else if (url is not null)
            PdfFile.UpdateUrl(url);
        
        PdfFile.UpdatePassword(PdfPassword);
        Loading = true;
        Error = null;
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, SinglePageMode, Config.UseProjectWorker);
    }

    #region Paging

    protected async Task FirstPageAsync()
    {
        if (PdfFile.Paging.FirstPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task LastPageAsync()
    {
        if (PdfFile.Paging.LastPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task NextPageAsync()
    {
        if (PdfFile.Paging.NextPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task PreviousPageAsync()
    {
        if (PdfFile.Paging.PreviousPage())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task PageNumberChanged(int value)
    {
        if (PdfFile.Paging.GotoPage(value))
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Zooming

    protected async Task ZoomInAsync()
    {
        if (PdfFile.Zooming.ZoomIn())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task ZoomOutAsync()
    {
        if (PdfFile.Zooming.ZoomOut())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task ResetZoomAsync()
    {
        if (PdfFile.Zooming.ResetZoom())
            await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Rotation

    protected async Task RotateClockwiseAsync()
    {
        PdfFile.Orientation.RotateClockwise();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task RotateCounterclockwiseAsync()
    {
        PdfFile.Orientation.RotateCounterClockwise();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task SwitchOrientationAsync()
    {
        PdfFile.Orientation.Flip();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Drawing

    protected async Task ToggleDrawingAsync()
    {
        PdfFile.DrawLayer.Toggle();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }
    
    protected async Task UpdatePenColorAsync(string color)
    {
        PdfFile.DrawLayer.UpdateColor(color);
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task UpdatePenThickness(int thickness)
    {
        PdfFile.DrawLayer.UpdateThickness(thickness);
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }

    protected async Task UndoLastStrokeAsync()
    {
        await PdfInterop.UndoLastStrokeAsync(ObjectReference!, PdfFile);
    }
    
    protected async Task ClearAllPageStrokesAsync()
    {
        await PdfInterop.ClearStrokesForPageAsync(ObjectReference!, PdfFile);
    }

    #endregion

    #region Searching

    protected async Task Search(string query)
    {
        PdfFile?.Search.UpdateSearchQuery(query);
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    protected async Task NextResult()
    {
        PdfFile?.Search.NextResult();

        if (PdfFile?.Search.CurrentSearchResult?.Page != PdfFile?.Paging.CurrentPage)
            PdfFile?.Paging.GotoPage(PdfFile.Search.CurrentSearchResult!.Page);

        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }
    
    protected async Task PreviousResult()
    {
        PdfFile?.Search.PreviousResult();

        if (PdfFile?.Search.CurrentSearchResult?.Page != PdfFile?.Paging.CurrentPage)
            PdfFile?.Paging.GotoPage(PdfFile.Search.CurrentSearchResult!.Page);

        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile!);
    }

    #endregion

    #region Other

    protected async Task DownloadDocumentAsync()
    {
        await PdfInterop.DownloadDocumentAsync(ObjectReference!, PdfFile!);
    }

    protected async Task PrintDocumentAsync()
    {
        await PdfInterop.PrintDocumentAsync(ObjectReference!, PdfFile!);
    }
    
    protected async Task ViewMetadataAsync()
    {
        await PdfInterop.ViewMetadataAsync(ObjectReference!, PdfFile!);
    }
    
    protected void ClearMetadata()
    {
        Metadata = null;
        StateHasChanged();
    }

    #endregion


    protected async Task UploadFile(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file.Size > Config.MaxPdfFileUploadSize)
        {
            // TODO: Handle file too large
            return;
        }

        if (file.ContentType != "application/pdf")
        {
            // TODO: Handle file too large
            return;
        }

        await using var stream = file.OpenReadStream(Config.MaxPdfFileUploadSize);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        
        var base64 = Convert.ToBase64String(ms.ToArray());
        Url = base64;
        PdfFile = new Pdf.Pdf("".GenerateRandomString(), Url!, PdfOrientation);
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, SinglePageMode, Config.UseProjectWorker);
        await OnFileUploaded.InvokeAsync(new PdfViewerFileUploaded
        {
            FileName = file.Name,
            Size = file.Size,
            Contents = ms.ToArray()
        });
    }
}