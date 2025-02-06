FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MudBlazor.PdfViewer.Docs/MudBlazor.PdfViewer.Docs.csproj", "MudBlazor.PdfViewer.Docs/"]
COPY ["MudBlazor.PdfViewer/MudBlazor.PdfViewer.csproj", "MudBlazor.PdfViewer/"]

RUN dotnet restore "MudBlazor.PdfViewer.Docs/MudBlazor.PdfViewer.Docs.csproj"
COPY . .
WORKDIR "/src/MudBlazor.PdfViewer.Docs"

RUN dotnet build "MudBlazor.PdfViewer.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "MudBlazor.PdfViewer.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MudBlazor.PdfViewer.Docs.dll"]
