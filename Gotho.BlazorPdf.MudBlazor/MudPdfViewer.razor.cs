using Microsoft.AspNetCore.Components;

namespace Gotho.BlazorPdf.MudBlazor;

public partial class MudPdfViewer : PdfViewer
{
    [Inject] private new MudPdfViewerConfig Config { get; set; } = default!;
}