using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPdf.Extensions;

namespace MudBlazorPdf;

public partial class MudPdf : MudComponentBase
{
    private ElementReference _element;
    private DotNetObjectReference<MudPdf>? _objectReference;
    private string? _id;
    private double _scale = 1.0;
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
}