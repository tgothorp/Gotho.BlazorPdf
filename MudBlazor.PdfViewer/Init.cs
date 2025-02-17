using MudBlazorPdf;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Init
{
    public static IServiceCollection AddMudBlazorPdfViewer(this IServiceCollection services, Action<MudPdfViewerConfig>? configure = null)
    {
        var config = new MudPdfViewerConfig();
        configure?.Invoke(config);

        services.AddSingleton(config);
        services.AddScoped<PdfInterop>();
        services.AddScoped<PdfInteropV2>();

        return services;
    }
}