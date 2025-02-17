using Microsoft.JSInterop;

internal class PdfInteropV2(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> js =
        new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Gotho.MudBlazor.PdfViewer/mudpdfviewer.js").AsTask());

    public async Task InitializeAsync(object objRef, string elementId)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("init", objRef, elementId);
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