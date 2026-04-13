using Gotho.BlazorPdf.MudBlazor.Config;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Gotho.BlazorPdf.MudBlazor;

public partial class MudPdfViewer : PdfViewer
{
    [Parameter] public MudPdfIconConfig Icons { get; set; } = new();
    [Parameter] public MudPdfColorConfig MudColors { get; set; } = new();
    [Parameter] public Variant PageFieldVariant { get; set; } = Variant.Filled;
    [Parameter] public Variant PasswordFieldVariant { get; set; } = Variant.Filled;
    [Parameter] public Variant SearchFieldVariant { get; set; } = Variant.Filled;
}