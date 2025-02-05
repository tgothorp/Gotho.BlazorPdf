using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPdf.Extensions;

namespace MudBlazorPdf;

public partial class MudPdfViewer : MudComponentBase
{
    private ElementReference _element;
    private DotNetObjectReference<MudPdfViewer>? _objectReference;
    private string? _id;
    private double _scale = 1.0;

    private int _maxZoomLevel = 17;
    private int _minZoomLevel = 1;
    private int _zoomLevel = 8;
    private string _zoomPercentage = "100%";

    private int _pageNumber = 0;
    private int _pageCount = 0;
    
    private Orientation _oldOrientation = Orientation.Portrait;
    private double _rotation = 0;

    [Parameter] public Orientation Orientation { get; set; } = Orientation.Portrait;
    [Parameter] public string? Url { get; set; }

    [Inject] private PdfInterop PdfInterop { get; set; } = default;

    protected override async Task OnInitializedAsync()
    {
        _objectReference ??= DotNetObjectReference.Create(this);
        _rotation = Orientation == Orientation.Portrait ? 0 : -90;
        _id ??= "".GenerateRandomString();

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await PdfInterop.InitializeAsync(_objectReference!, _id, _scale, _rotation, Url!);

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public void DocumentLoaded(PdfViewerModel? pdfViewerModel)
    {
        if (pdfViewerModel is null)
            return;

        _pageNumber = pdfViewerModel.PageNumber;
        _pageCount = pdfViewerModel.PagesCount;

        StateHasChanged();

        // if (OnDocumentLoaded.HasDelegate)
        //     OnDocumentLoaded.InvokeAsync(new PdfViewerEventArgs(pageNumber, pagesCount));
    }

    [JSInvokable]
    public void SetPdfViewerMetaData(PdfViewerModel? pdfViewerModel)
    {
        if (pdfViewerModel is null)
            return;

        _pageNumber = pdfViewerModel.PageNumber;
        _pageCount = pdfViewerModel.PagesCount;

        // if (OnPageChanged.HasDelegate)
        //     OnPageChanged.InvokeAsync(new PdfViewerEventArgs(pageNumber, pagesCount));
    }

    private async Task FirstPageAsync() => await PdfInterop.FirstPageAsync(_objectReference!, _id!);
    private async Task LastPageAsync() => await PdfInterop.LastPageAsync(_objectReference!, _id!);
    private async Task NextPageAsync() => await PdfInterop.NextPageAsync(_objectReference!, _id!);
    private async Task PreviousPageAsync() => await PdfInterop.PreviousPageAsync(_objectReference!, _id!);

    private int GetZoomPercentage(int zoomLevel) =>
        zoomLevel switch
        {
            1 => 25,
            2 => 33,
            3 => 50,
            4 => 67,
            5 => 75,
            6 => 80,
            7 => 90,
            8 => 100,
            9 => 110,
            10 => 125,
            11 => 150,
            12 => 175,
            13 => 200,
            14 => 250,
            15 => 300,
            16 => 400,
            17 => 500,
            _ => 100
        };

    private async Task ZoomInAsync()
    {
        if (_zoomLevel == _maxZoomLevel)
            return;

        _zoomLevel += 1;
        var zp = GetZoomPercentage(_zoomLevel);
        _zoomPercentage = $"{zp}%";
        _scale = 0.01 * zp;
        await PdfInterop.ZoomInOutAsync(_objectReference!, _id!, _scale);
    }

    private async Task ZoomOutAsync()
    {
        if (_zoomLevel == _minZoomLevel)
            return;

        _zoomLevel -= 1;
        var zp = GetZoomPercentage(_zoomLevel);
        _zoomPercentage = $"{zp}%";
        _scale = 0.01 * zp;
        await PdfInterop.ZoomInOutAsync(_objectReference!, _id!, _scale);
    }
    
    private async Task RotateClockwiseAsync()
    {
        _rotation += 90;
        _rotation = _rotation.Equals(360) ? 0 : _rotation;
        await PdfInterop.RotateAsync(_objectReference!, _id!, _rotation);

        SetOrientation();
    }

    private async Task RotateCounterclockwiseAsync()
    {
        _rotation += 90;
        _rotation = _rotation.Equals(360) ? 0 : _rotation;
        await PdfInterop.RotateAsync(_objectReference!, _id!, _rotation);

        SetOrientation();
    }

    private void SetOrientation()
    {
        _oldOrientation = _rotation switch
        {
            0 => Orientation = Orientation.Portrait,
            -90 => Orientation = Orientation.Landscape,
            _ => _oldOrientation
        };
    }

    private async Task SwitchOrientationAsync()
    {
        _oldOrientation = Orientation;
        Orientation = Orientation == Orientation.Portrait ? Orientation.Landscape : Orientation.Portrait;
        _rotation = Orientation == Orientation.Portrait ? 0 : -90;
        
        await PdfInterop.RotateAsync(_objectReference!, _id!, _rotation);
    }
}