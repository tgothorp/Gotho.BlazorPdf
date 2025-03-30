using Gotho.BlazorPdf.MudBlazor.Config;
using Microsoft.AspNetCore.Components;

namespace Gotho.BlazorPdf.MudBlazor;

public partial class MudPdfViewer : PdfViewer
{
    [Parameter] public MudPdfIconConfig Icons { get; set; } = new();
    [Parameter] public MudPdfColorConfig PdfColors { get; set; } = new();
}