FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Clients/src/Crm.Clients.Domain/Crm.Clients.Domain.csproj services/Crm.Clients/src/Crm.Clients.Domain/
COPY services/Crm.Clients/src/Crm.Clients.Application/Crm.Clients.Application.csproj services/Crm.Clients/src/Crm.Clients.Application/
COPY services/Crm.Clients/src/Crm.Clients.Infrastructure/Crm.Clients.Infrastructure.csproj services/Crm.Clients/src/Crm.Clients.Infrastructure/
COPY services/Crm.Clients/src/Crm.Clients.Api/Crm.Clients.Api.csproj services/Crm.Clients/src/Crm.Clients.Api/

RUN dotnet restore services/Crm.Clients/src/Crm.Clients.Api/Crm.Clients.Api.csproj

COPY shared/ shared/
COPY services/Crm.Clients/ services/Crm.Clients/

RUN dotnet publish services/Crm.Clients/src/Crm.Clients.Api/Crm.Clients.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Clients.Api.dll"]
