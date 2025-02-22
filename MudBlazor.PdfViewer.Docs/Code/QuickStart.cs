namespace MudBlazor.PdfViewer.Docs.Code;

public static class QuickStart
{
    public static string Package => 
@"dotnet add package Gotho.MudBlazor.PdfViewer --version 2.0.0";
    
    public static string ProgramCs =>
@"builder.Services.AddMudBlazorPdfViewer();";
    
    public static string AppRazor =>
@"<link href=""_content/Gotho.MudBlazor.PdfViewer/mudpdf.min.css"" rel=""stylesheet""/>";
    
    public static string ImportRazor =>
@"@using MudBlazor.PdfViewer";
    
    public static string Minimum =>
@"<MudPdfViewer Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""/>";
}