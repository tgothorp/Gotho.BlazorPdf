FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Gotho.BlazorPdf.Docs/Gotho.BlazorPdf.Docs.csproj", "Gotho.BlazorPdf.Docs/"]
COPY ["Gotho.BlazorPdf/Gotho.BlazorPdf.csproj", "Gotho.BlazorPdf/"]

RUN dotnet restore "Gotho.BlazorPdf.Docs/Gotho.BlazorPdf.Docs.csproj"
COPY . .

WORKDIR /src/Gotho.BlazorPdf
RUN apt-get update && apt-get install -y nodejs npm && npm install

WORKDIR "/src/Gotho.BlazorPdf.Docs"
RUN dotnet build "Gotho.BlazorPdf.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Gotho.BlazorPdf.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gotho.BlazorPdf.Docs.dll"]
