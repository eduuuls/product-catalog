#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ProductCatalog.Crawler.ProductReviews/ProductCatalog.Crawler.ProductReviews.csproj", "ProductCatalog.Crawler.ProductReviews/"]
COPY ["ProductCatalog.Infra.CrossCutting/ProductCatalog.Infra.CrossCutting.csproj", "ProductCatalog.Infra.CrossCutting/"]
COPY ["ProductCatalog.Infra.CrossCutting.IoC/ProductCatalog.Infra.CrossCutting.IoC.csproj", "ProductCatalog.Infra.CrossCutting.IoC/"]
COPY ["ProductCatalog.Domain/ProductCatalog.Domain.csproj", "ProductCatalog.Domain/"]
COPY ["ProductCatalog.Infra.Data.Persistance/ProductCatalog.Infra.Data.Persistance.csproj", "ProductCatalog.Infra.Data.Persistance/"]
COPY ["ProductCatalog.Infra.CrossCutting.Bus/ProductCatalog.Infra.CrossCutting.Bus.csproj", "ProductCatalog.Infra.CrossCutting.Bus/"]
COPY ["ProductCatalog.Infra.Data/ProductCatalog.Infra.Data.csproj", "ProductCatalog.Infra.Data/"]
COPY ["ProductCatalog.Application/ProductCatalog.Application.csproj", "ProductCatalog.Application/"]
RUN dotnet restore "ProductCatalog.Crawler.ProductReviews/ProductCatalog.Crawler.ProductReviews.csproj"
COPY . .
WORKDIR "/src/ProductCatalog.Crawler.ProductReviews"
RUN dotnet build "ProductCatalog.Crawler.ProductReviews.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductCatalog.Crawler.ProductReviews.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalog.Crawler.ProductReviews.dll"]