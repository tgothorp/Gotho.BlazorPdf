# Blazor PDF

Blazor PDF is a simple yet powerful PDF viewer build for Blazor.

## Installation

Grab the package from nuget.org;

```
dotnet add package Gotho.BlazorPdf
```

### Configure

Add the following to your application startup in `Program.cs`;

```
builder.Services.AddBlazorPdfViewer();
```

Update your `App.razor` file to import the required CSS;

```
<link href="_content/Gotho.BlazorPdf/blazorpdf.min.css" rel="stylesheet"/>
```

Then, add the namespace to your `_Imports.razor` file.

```
@using Gotho.BlazorPdf
```

You should now be fully setup to use the `PdfViewer` component in your code!

## Prerequisites

In order to use this package, your project must target .NET Version 8.0 or 9.0

## Usage & Examples

Please see the [documentation site](https://mudpdf.info/docs) for detailed examples
