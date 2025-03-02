using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPdf.Extensions;

namespace MudBlazorPdf;

public partial class MudPdfViewer : ComponentBase
{
    private bool _loading = true;

    private ElementReference _element;
    private DotNetObjectReference<MudPdfViewer>? _objectReference;
    public Pdf.Pdf PdfFile { get; private set; }
    
    // private string? _id;
    // private double _scale = 1.0;
    //
    // private int _maxZoomLevel = 17;
    // private int _minZoomLevel = 1;
    // private int _defaultZoomLevel = 8;
    // private int _zoomLevel = 8;
    // private string _zoomPercentage = "100%";
    //
    // private int _pageNumber = 0;
    // private int _pageCount = 0;
    //
    // private PdfOrientation _oldPdfOrientation = PdfOrientation.Portrait;
    // private double _rotation = 0;
    //
    // private bool _toggleThumbnails = true;
    //
    // private string? _password = null;
    // private InputType _passwordInputType = InputType.Password;
    // private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    // private bool _passwordVisible = false;
    //
    // private bool PdfErrored => _pdfError != null;
    // private PdfError? _pdfError = null;

    /// <summary>
    /// Sets the display orientation of the PDF document
    /// </summary>
    /// <remarks>
    /// Defaults to <c>Orientation.Portrait</c>
    /// </remarks>
    [Parameter]
    public PdfOrientation PdfOrientation { get; set; } = PdfOrientation.Portrait;

    /// <summary>
    /// Determines the height of the PDF viewer when <c>SinglePageMode</c> is set to <c>true</c>.
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
    [Inject] private MudPdfViewerConfig Config { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _objectReference ??= DotNetObjectReference.Create(this);
        PdfFile = new Pdf.Pdf("".GenerateRandomString(), Url, PdfOrientation);
        // _rotation = PdfOrientation == PdfOrientation.Portrait ? 0 : -90;
        // _id ??= "".GenerateRandomString();
        // _toggleThumbnails = !HideThumbnails;
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await PdfInterop.InitializeAsync(_objectReference!, PdfFile, SinglePageMode);

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public void DocumentLoaded(PdfViewerModel? pdfViewerModel)
    {
        _loading = false;

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

    // [JSInvokable]
    // public void PdfViewerError(PdfViewerError error)
    // {
    //     _loading = false;
    //     _pdfError = error.Name switch
    //     {
    //         "PasswordException" => new PdfError { ErrorType = PdfErrorType.PasswordRequired, Message = error.Message?.ToLower() == "no password given" ? null : error.Message },
    //         _ => new PdfError { ErrorType = PdfErrorType.Error, Message = error.Message }
    //     };
    //
    //     StateHasChanged();
    // }

    #region Paging

    private async Task FirstPageAsync()
    {
        if (PdfFile.Paging.FirstPage())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    private async Task LastPageAsync()
    {
        if (PdfFile.Paging.LastPage())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    private async Task NextPageAsync()
    {
        if (PdfFile.Paging.NextPage())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    private async Task PreviousPageAsync()
    {
        if (PdfFile.Paging.PreviousPage())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    protected async Task PageNumberChanged(int value)
    {
        if (PdfFile.Paging.GotoPage(value))
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    #endregion

    #region Zooming

    protected async Task ZoomInAsync()
    {
        if (PdfFile.Zooming.ZoomIn())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    protected async Task ZoomOutAsync()
    {
        if (PdfFile.Zooming.ZoomOut())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    protected async Task ResetZoomAsync()
    {
        if (PdfFile.Zooming.ResetZoom())
            await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    #endregion

    #region Rotation

    protected async Task RotateClockwiseAsync()
    {
        PdfFile.Orientation.RotateClockwise();
        await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    protected async Task RotateCounterclockwiseAsync()
    {
        PdfFile.Orientation.RotateCounterClockwise();
        await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    protected async Task SwitchOrientationAsync()
    {
        PdfFile.Orientation.Flip();
        await PdfInterop.UpdateAsync(_objectReference!, PdfFile);
    }

    #endregion

    // private async Task PrintDocumentAsync()
    // {
    //     await PdfInterop.PrintDocumentAsync(_objectReference!, _id!);
    // }
    //
    // private async Task DownloadDocumentAsync()
    // {
    //     await PdfInterop.DownloadDocumentAsync(_objectReference!, _id!);
    // }
    //
    // private async Task ReloadPdfAsync()
    // {
    //     if (_pdfError is not null && _pdfError.ErrorType == PdfErrorType.PasswordRequired && string.IsNullOrEmpty(_password))
    //     {
    //         _pdfError.Message = "Please supply a password.";
    //         StateHasChanged();
    //         return;
    //     }
    //
    //     _loading = true;
    //     _pdfError = null;
    //     StateHasChanged();
    //
    //     await PdfInterop.InitializeAsync(_objectReference!, _id!, Url!, _scale, _rotation, SinglePageMode, _password);
    // }
    //
    // private void PeekPassword()
    // {
    //     if (_passwordVisible)
    //     {
    //         _passwordVisible = false;
    //         _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    //         _passwordInputType = InputType.Password;
    //     }
    //     else {
    //         _passwordVisible = true;
    //         _passwordInputIcon = Icons.Material.Filled.Visibility;
    //         _passwordInputType = InputType.Text;
    //     }
    // }
    //
    //
    // private void ToggleThumbnails()
    // {
    //     _toggleThumbnails = !_toggleThumbnails;
    //     StateHasChanged();
    // }
    //
    // private string ThumbnailClass()
    // {
    //     return _toggleThumbnails
    //         ? "mudpdf_thumbnails"
    //         : "mudpdf_thumbnails d-none";
    // }
    //
    private string ColorStyle()
    {
        return $"background-color: {Config.Colors.Background}";
    }
    
    private string ScrollStyle()
    {
        return $"height: {Height}";
    }
}