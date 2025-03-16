namespace MudBlazor.PdfViewer.Docs.Code;

public static class MudBlazorExample
{
    public static string Example =>
        @"<MudPdfViewer Url=""https://raw.githubusercontent.com/tgothorp/MudBlazor.PdfViewer/refs/heads/main/files/test_pdf_document.pdf""/>";
    
    public static string Package => 
        @"dotnet add package Gotho.BlazorPdf.MudBlazor";
    
    public static string ProgramCs =>
        @"builder.Services.AddMudBlazorPdfViewer();";
    
    public static string AppRazor =>
        @"<link href=""_content/Gotho.BlazorPdf.MudBlazor/blazorpdf_mudblazor.min.css"" rel=""stylesheet""/>";
    
    public static string ImportRazor =>
        @"@using Gotho.BlazorPdf.MudBlazor;";
    
    public static string Minimum =>
        @"<MudPdfViewer Url=""https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf""/>";
}