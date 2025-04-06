using Gotho.BlazorPdf.Docs.Components;
using MudBlazor.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(opt =>
{
    opt.SetResourceBuilder(ResourceBuilder.CreateDefault());
    opt.AddOtlpExporter();

    if (builder.Environment.IsDevelopment())
    {
        opt.AddConsoleExporter();
    }
});

builder.Services.AddOpenTelemetry().WithTracing(opt =>
{
    opt
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter();

    if (builder.Environment.IsDevelopment())
    {
        opt.AddConsoleExporter();
    }
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddBlazorPdfViewer();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHealthChecks("/healthz");
app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Blazor PDF docs site starting...");

app.Run();