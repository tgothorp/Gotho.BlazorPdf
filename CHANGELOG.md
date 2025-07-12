# CHANGELOG

All notable changes and releases will be documented here

## [2025-07-12] - V1.1.1 (Latest Release)

- Fixed zooming breaking single page mode

## [2025-04-05] - V1.1.0

- Implemented ability to draw on PDF documents.
- Printing documents will also include any drawings made.
- Configuration of labels and colors can now be done at runtime in order to support dynamically switching languages / themes.
- Added `BlazorPdfLocalizedStrings` and `BlazorPdfColors`.
- Removed label and color configuration when configuring BlazorPdf with `AddBlazorPdfViewer()`.
- Improved PdfViewer component to better stack the text layer on top of the document canvas.
- Improved the look of the PdfViewer component on smaller screens.
- Added additional comments to some of the PdfViewer component methods.
- Fixed text layer behaving oddly when resizing the window.

## [2025-03-29] - V1.0.3

- Added `LoadPdfAsync` method as an alternative to specifying a URL parameter
- Fixed downloading and printing not working for PDFs loaded as a base64 string

## [2025-03-23] - V1.0.2

- Added fix to allow base64 encoded PDFs to be loaded
- Added `UseProjectWorker` config option to allow Blazor PDF to work with MAUI Hybrid applications
- Mark some internal classes as internal
- Fix thumbnails displaying incorrectly on very wide displays
- Updated readme to point to new url
- Dedicated core test project now docs site is using packages
- Fixed some namespaces using the old project name

## [2025-03-21] - V1.0.1

- Rename project from MudPdf to Blazor PDF
- Complete rewrite of core functionality to remove MudBlazor dependency
- New package to provide dedicated MudBlazor integration
- Rewrite of JavaScript interop code
- Rewrite of PDF Viewer in vanilla HTML & CSS