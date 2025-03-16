# Blazor PDF (MudBlazor Extension)

This packages provides a MudBlazor-specific PDF viewing experience for use with [Blazor PDF](https://blazorpdf.info)

## Prerequisites

In order to use this package, your project will need to be configured with the following;

- **MudBlazor** Version 8.0.0 or later
- **.NET** Version 8.0 or 9.0

## Installation

Grab the package from nuget.org;

```
dotnet add package Gotho.BlazorPdf.MudBlazor
```

### Configure

Add the following to your application startup in `Program.cs`;

```
builder.Services.AddMudBlazorPdfViewer();
```

Update your `App.razor` file to import the required CSS;

```
<link href="_content/Gotho.BlazorPdf.MudBlazor/blazorpdf_mudblazor.min.css" rel="stylesheet"/>
```

Then, add the namespace to your `_Imports.razor` file.

```
@using Gotho.BlazorPdf.MudBlazor
```

You should now be fully setup to use the `MudPdfViewer` component in your code!

## Usage & Examples

Please see the [documentation site](https://mudpdf.info/docs) for detailed examples
