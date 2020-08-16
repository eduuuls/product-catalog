#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY *.sln ./
COPY ["ProductCatalog.Api/ProductCatalog.Api.csproj", "ProductCatalog.Api/"]
COPY ["ProductCatalog.Infra.CrossCutting.IoC/ProductCatalog.Infra.CrossCutting.IoC.csproj", "ProductCatalog.Infra.CrossCutting.IoC/"]
COPY ["ProductCatalog.Domain/ProductCatalog.Domain.csproj", "ProductCatalog.Domain/"]
COPY ["ProductCatalog.Infra.Data/ProductCatalog.Infra.Data.csproj", "ProductCatalog.Infra.Data/"]
COPY ["ProductCatalog.Infra.Data.Persistance/ProductCatalog.Infra.Data.Persistance.csproj", "ProductCatalog.Infra.Data.Persistance/"]
COPY ["ProductCatalog.Application/ProductCatalog.Application.csproj", "ProductCatalog.Application/"]
RUN dotnet restore "ProductCatalog.Api/ProductCatalog.Api.csproj"
COPY . .
WORKDIR "/src/ProductCatalog.Api"
RUN dotnet build "ProductCatalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductCatalog.Api.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]