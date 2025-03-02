using Microsoft.AspNetCore.Components;
using MudBlazorPdf;

namespace Gotho.BlazorPdf.MudBlazor;

public partial class MudPdfViewer : PdfViewer
{
    [Inject] private MudPdfViewerConfig Config { get; set; } = default!;
}