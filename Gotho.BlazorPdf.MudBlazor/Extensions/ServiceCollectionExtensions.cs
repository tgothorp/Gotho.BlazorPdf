using Gotho.BlazorPdf;
using MudBlazorPdf;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMudBlazorPdfViewer(this IServiceCollection services,
        Action<PdfViewerConfig>? baseConfig = null,
        Action<MudPdfViewerConfig>? mudConfig = null)
    {
        var config = new MudPdfViewerConfig();
        mudConfig?.Invoke(config);

        services.AddSingleton(config);
        services.AddBlazorPdfViewer(baseConfig);
        return services;
    }
}