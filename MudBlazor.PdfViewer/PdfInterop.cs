using Microsoft.JSInterop;

namespace MudBlazorPdf;

internal class PdfInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> js = 
        new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Gotho.MudBlazor.PdfViewer/mudpdf.js").AsTask());

    public async Task FirstPageAsync(object objRef, string elementId)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("firstPage", objRef, elementId);
    }

    public async Task GotoPageAsync(object objRef, string elementId, int gotoPageNum)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("gotoPage", objRef, elementId, gotoPageNum);
    }

    public async Task InitializeAsync(object objRef, string elementId, double scale, double rotation, string url, bool showThumbs)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("initialize", objRef, elementId, scale, rotation, url, showThumbs);
    }

    public async Task LastPageAsync(object objRef, string elementId)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("lastPage", objRef, elementId);
    }

    public async Task NextPageAsync(object objRef, string elementId)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("nextPage", objRef, elementId);
    }

    public async Task PreviousPageAsync(object objRef, string elementId)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("previousPage", objRef, elementId);
    }

    public async Task PrintAsync(object objRef, string elementId, string url)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("print", objRef, elementId, url);
    }

    public async Task RotateAsync(object objRef, string elementId, double rotation)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("rotate", objRef, elementId, rotation);
    }

    public async Task ZoomInOutAsync(object objRef, string elementId, double scale)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("zoomInOut", objRef, elementId, scale);
    }
    
    public async Task RenderThumbs(object objRef, string elementId)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("renderThumbnails", objRef, elementId);
    }

    
    public async ValueTask DisposeAsync()
    {
        if (js.IsValueCreated)
        {
            var module = await js.Value;
            await module.DisposeAsync();
        }
    }
}