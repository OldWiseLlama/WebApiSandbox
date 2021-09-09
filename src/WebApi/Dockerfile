FROM mcr.microsoft.com/dotnet/aspnet:5.0 as base
WORKDIR /app
ENV WEB_API_ENVIRONMENT=Development
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source
COPY *.csproj ./
RUN dotnet restore "./WebApi.csproj"
COPY . .
RUN dotnet build "./WebApi.csproj" -c Release  -o /app

FROM build as publish 
RUN dotnet publish "./WebApi.csproj" -c Release  -o /app

FROM base as final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebApi.dll"]