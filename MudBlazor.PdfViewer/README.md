# Gotho.MudBlazor.PdfViewer

A port of the BlazorBootstrap PDF viewer to work with MudBlazor

## Installation

Grab the package from nuget.org;

```
dotnet add package Gotho.MudBlazor.PdfViewer --version 1.1.0
```

### Configure

Add the following to your application startup in `Program.cs`;

```
builder.Services.AddMudBlazorPdfViewer();
```

Update your `App.razor` file to import the required CSS;

```
<link href="_content/Gotho.MudBlazor.PdfViewer/mudpdf.min.css" rel="stylesheet"/>
```

Then, add the namespace to your `_Imports.razor` file.

```
@using MudBlazor.PdfViewer
```

You should now be fully setup to use the `MudPdfViewer` component in your code!

## Prerequisites

In order to use this package, your project will need to be configured with the following;

- **MudBlazor** Version 8.0.0 or later
- **.NET** Version 8.0 or 9.0

## Usage & Examples

Please see the [documentation site](https://mudpdf.info) for detailed examples
