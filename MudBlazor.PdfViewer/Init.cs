using MudBlazorPdf;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Init
{
    public static IServiceCollection AddMudBlazorPdfViewer(this IServiceCollection services)
    {
        services.AddScoped<PdfInterop>();
        
        return services;
    }
}