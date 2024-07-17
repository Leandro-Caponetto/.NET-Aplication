#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye AS build
WORKDIR /src
COPY ["src/Presentation.Site/Presentation.Site.csproj", "src/Presentation.Site/"]
COPY ["src/Core.RequestsHTTP/Core.RequestsHTTP.csproj", "src/Core.RequestsHTTP/"]
COPY ["src/Core.Data/Core.Data.csproj", "src/Core.Data/"]
COPY ["src/Core.Entities/Core.Entities.csproj", "src/Core.Entities/"]
COPY ["src/Core.RequestsHTTP.Models/Core.RequestsHTTP.Models.csproj", "src/Core.RequestsHTTP.Models/"]
COPY ["src/Core/Core.csproj", "src/Core/"]
COPY ["src/Core.Data.EF/Core.Data.EF.csproj", "src/Core.Data.EF/"]
RUN dotnet restore "src/Presentation.Site/Presentation.Site.csproj"
COPY . .
WORKDIR "/src/src/Presentation.Site"
RUN dotnet build "Presentation.Site.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Presentation.Site.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Presentation.Site.dll"]