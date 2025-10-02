using Gotho.BlazorPdf.MudBlazor.Config;
using Microsoft.AspNetCore.Components;

namespace Gotho.BlazorPdf.MudBlazor;

public partial class MudPdfViewer : PdfViewer
{
    [Parameter] public MudPdfIconConfig Icons { get; set; } = new();
    [Parameter] public MudPdfColorConfig MudColors { get; set; } = new();

    /// <summary>
    /// Hides the print button
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    [Parameter]
    public bool HidePrintButton { get; set; } = false;

    /// <summary>
    /// Hides the download button
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    [Parameter]
    public bool HideDownloadButton { get; set; } = false;

    /// <summary>
    /// Hides the menu completely
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    [Parameter]
    public bool HideMenu { get; set; } = false;
}