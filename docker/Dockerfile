FROM mcr.microsoft.com/dotnet/sdk:6.0.405-alpine3.17 AS build
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
RUN dotnet publish "Presentation.Site.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:6.0.13-alpine3.17 AS final
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
 
# Install tzdata package
RUN apk add --no-cache tzdata
# Change timezone to local time
RUN TZ="America/Argentina/Buenos_Aires"
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN export TZ

RUN apk add --no-cache icu-libs

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Presentation.Site.dll", "--urls", "http://0.0.0.0:8080"]