using Gotho.BlazorPdf.MudBlazor;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMudBlazorPdfViewer(this IServiceCollection services, Action<MudPdfViewerConfig>? mudConfig = null)
    {
        var config = new MudPdfViewerConfig();
        mudConfig?.Invoke(config);

        services.AddSingleton(config);
        services.AddBlazorPdfViewer(opt => { opt.Labels = config.Labels; });

        return services;
    }
}