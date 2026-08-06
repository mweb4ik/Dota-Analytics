FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["backend/DotaAnalytics.Api/DotaAnalytics.Api.csproj", "backend/DotaAnalytics.Api/"]
COPY ["backend/DotaAnalytics.Infrastructure/DotaAnalytics.Infrastructure.csproj", "backend/DotaAnalytics.Infrastructure/"]

RUN dotnet restore "backend/DotaAnalytics.Api/DotaAnalytics.Api.csproj"

COPY . .

WORKDIR "/src/backend/DotaAnalytics.Api"
RUN dotnet build "DotaAnalytics.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotaAnalytics.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DotaAnalytics.Api.dll"]