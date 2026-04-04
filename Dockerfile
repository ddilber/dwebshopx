FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/dWebShop.Web/dWebShop.Web.csproj", "src/dWebShop.Web/"]
COPY ["src/dWebShop.Application/dWebShop.Application.csproj", "src/dWebShop.Application/"]
COPY ["src/dWebShop.Domain/dWebShop.Domain.csproj", "src/dWebShop.Domain/"]
COPY ["src/dWebShop.Infrastructure/dWebShop.Infrastructure.csproj", "src/dWebShop.Infrastructure/"]
RUN dotnet restore "src/dWebShop.Web/dWebShop.Web.csproj"

COPY . .
WORKDIR "/src/src/dWebShop.Web"
RUN dotnet publish "dWebShop.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p wwwroot/product_images/brands wwwroot/product_docs logs

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "dWebShop.Web.dll"]
