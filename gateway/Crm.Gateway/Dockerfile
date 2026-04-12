FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY gateway/Crm.Gateway/Crm.Gateway.csproj gateway/Crm.Gateway/

RUN dotnet restore gateway/Crm.Gateway/Crm.Gateway.csproj

COPY shared/ shared/
COPY gateway/ gateway/

RUN dotnet publish gateway/Crm.Gateway/Crm.Gateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Gateway.dll"]
