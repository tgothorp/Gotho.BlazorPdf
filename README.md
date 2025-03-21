<div align="center">

<h1>Blazor PDF (Formally Mud PDF)</h1>

<div><img src="img/square_small.png" width="230" alt="MudPDF" /></div>

<div><a href="https://mudpdf.info">Demo</a> | <a href="https://mudpdf.info/docs/quickstart">Quickstart</a> | <a href="https://mudpdf.info/docs">Documentation</a></div>

Originally a port of the BlazorBootstrap PDF viewer for the [MudBlazor](https://mudblazor.com) component library. Blazor PDF is a simple yet powerful PDF viewer for Blazor framework.

![NuGet Version](https://img.shields.io/nuget/v/Gotho.MudBlazor.PdfViewer)
![NuGet Downloads](https://img.shields.io/nuget/dt/Gotho.MudBlazor.PdfViewer)
![GitHub License](https://img.shields.io/github/license/tgothorp/MudBlazor.PdfViewer)


</div>

## Contents

- [Installation](#installation)
  * [Configuration](#Configuration)
- [Prerequisites](#prerequisites)
- [Usage & Examples](#usage---examples)
- [MudBlazor Integration](#mudblazor-integration)
- [Contributing](#contributing)
- [Icons](#icons)
- [License](#license)

## Installation

Grab the package from [nuget.org](https://www.nuget.org/packages/Gotho.MudBlazor.PdfViewer/1.0.1#readme-body-tab);

```
dotnet add package Gotho.BlazorPdf
```

### Configuration

Add the following to your application startup in `Program.cs`;

```
builder.Services.AddBlazorPdfViewer();
```

Update your `App.razor` file to import the required CSS;

```
<link href="_content/Gotho.BlazorPdf/blazorpdf.min.css" rel="stylesheet"/>
```

Then, add the namespace to your `_Imports.razor` file;

```
@using Gotho.BlazorPdf
```

You should now be fully setup to use the `PdfViewer` component in your code!


## Prerequisites

In order to use this package, your project will need to be configured with the following;

- **.NET** Version 8.0 or 9.0

## Usage & Examples

Please see the [documentation site](https://mudpdf.info/docs) for detailed examples

## MudBlazor Integration

This project was previously known as MudPdf / MudBlazor.PdfViewer. This repo was updated and renamed from Version 3.0.0 onwards to remove the MudBlazor dependency such that the library can be used by any Blazor project.

However, in keeping with the original spirit of this project, a separate, dedicated package for MudBlazor is available.

See the [documentation website](https://blazorpdf.info/docs/mudblazor) for installation & usage details.

## Contributing

Feel free to make contributions, see the `CONTRIBUTING.md` file for more information.

## Icons

This project uses the excellent [feather icons](https://github.com/feathericons/feather) icon library.

## License

Licensed under Apache-2.0

This library contains code from the `PdfViewer` Blazor component provided by the [Blazor Bootstrap](https://github.com/vikramlearning/blazorbootstrap/tree/main/blazorbootstrap) component library which is licensed under the Apache 2.0 license. 

See `NOTICE.md` for more information.