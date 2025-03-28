using Gotho.BlazorPdf;
using Gotho.BlazorPdf.Config;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorPdfViewer(this IServiceCollection services, Action<PdfViewerConfig>? configure = null)
    {
        var config = new PdfViewerConfig();
        configure?.Invoke(config);

        services.AddSingleton(config);
        services.AddScoped<PdfInterop>();

        return services;
    }
}