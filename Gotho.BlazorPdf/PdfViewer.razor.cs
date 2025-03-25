using Gotho.BlazorPdf.Extensions;
using Gotho.BlazorPdf.Pdf;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gotho.BlazorPdf;

public partial class PdfViewer : ComponentBase
{
    protected bool Loading = true;
    protected ElementReference Element;
    protected DotNetObjectReference<PdfViewer>? ObjectReference;
    protected PdfError? Error;
    protected string? PdfPassword;

    private Pdf.Pdf PdfFile { get; set; } = null!;

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
    [EditorRequired, Parameter]
    public string? Url { get; set; }

    /// <summary>
    /// Hides the thumbnail bar as well as the option to display it
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    [Parameter]
    public bool HideThumbnails { get; set; } = false;

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

    [Inject] private PdfInterop PdfInterop { get; set; } = default!;
    [Inject] protected PdfViewerConfig Config { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ObjectReference ??= DotNetObjectReference.Create(this);
        PdfFile = new Pdf.Pdf("".GenerateRandomString(), Url!, PdfOrientation);

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, SinglePageMode, Config.UseProjectWorker);

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public void DocumentLoaded(PdfViewerModel? pdfViewerModel)
    {
        Loading = false;

        if (pdfViewerModel is null)
            return;

        PdfFile.Paging.Update(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages);
        StateHasChanged();

        if (OnDocumentLoaded.HasDelegate)
            OnDocumentLoaded.InvokeAsync(new PdfViewerEventArgs(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages));
    }

    [JSInvokable]
    public void SetPdfViewerMetaData(PdfViewerModel? pdfViewerModel)
    {
        if (pdfViewerModel is null)
            return;

        PdfFile.Paging.Update(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages);
        StateHasChanged();

        if (OnPageChanged.HasDelegate)
            OnPageChanged.InvokeAsync(new PdfViewerEventArgs(pdfViewerModel.CurrentPage, pdfViewerModel.TotalPages));
    }

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

    protected async Task DownloadDocumentAsync()
    {
        await PdfInterop.DownloadDocumentAsync(ObjectReference!, PdfFile);
    }

    protected async Task ReloadPdfAsync()
    {
        if (Error is not null && Error.ErrorType == PdfErrorType.PasswordRequired && string.IsNullOrEmpty(PdfPassword))
        {
            Error.Message = "Please supply a password.";
            StateHasChanged();
            return;
        }

        PdfFile.UpdatePassword(PdfPassword);
        Loading = true;
        Error = null;
        StateHasChanged();

        await PdfInterop.InitializeAsync(ObjectReference!, PdfFile, SinglePageMode, Config.UseProjectWorker);
    }

    protected async Task PrintDocumentAsync()
    {
        await PdfInterop.PrintDocumentAsync(ObjectReference!, PdfFile);
    }

    protected async Task ToggleDrawing()
    {
        PdfFile.ToggleDrawing();
        await PdfInterop.UpdateAsync(ObjectReference!, PdfFile);
    }
}