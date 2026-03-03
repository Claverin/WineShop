FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY WineShop.sln ./
COPY WineShop/WineShop.csproj WineShop/
RUN dotnet restore WineShop.sln

COPY . .
WORKDIR /src/WineShop
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WineShop.dll"]