using Gotho.BlazorPdf.Pdf;
using Microsoft.JSInterop;

internal class PdfInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> js =
        new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Gotho.BlazorPdf/blazorpdf.min.js").AsTask());

    public async Task InitializeAsync(object objRef, Pdf pdf, bool scrollMode, bool useProjectWorker)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("initPdfViewer", objRef, pdf.GetPdfState(), scrollMode, useProjectWorker);
    }

    public async Task UpdateAsync(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("updatePdf", objRef, pdf.GetPdfState());
    }

    public async Task PrintDocumentAsync(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("printDocument", objRef, pdf.Id);
    }

    public async Task DownloadDocumentAsync(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("downloadDocument", objRef, pdf.Id);
    }
    
    public async Task ViewMetadataAsync(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("viewMetadata", objRef, pdf.Id);
    }

    public async Task UndoLastStrokeAsync(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("undoLastStroke", objRef, pdf.Id);
    }
    
    public async Task ClearStrokesForPageAsync(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("clearStrokesForPage", objRef, pdf.Id);
    }
    
    public async Task ClearSearchResults(object objRef, Pdf pdf)
    {
        var module = await js.Value;
        await module.InvokeVoidAsync("clearSearchResults", objRef, pdf.Id);
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