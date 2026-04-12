FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Deals/src/Crm.Deals.Domain/Crm.Deals.Domain.csproj services/Crm.Deals/src/Crm.Deals.Domain/
COPY services/Crm.Deals/src/Crm.Deals.Application/Crm.Deals.Application.csproj services/Crm.Deals/src/Crm.Deals.Application/
COPY services/Crm.Deals/src/Crm.Deals.Infrastructure/Crm.Deals.Infrastructure.csproj services/Crm.Deals/src/Crm.Deals.Infrastructure/
COPY services/Crm.Deals/src/Crm.Deals.Api/Crm.Deals.Api.csproj services/Crm.Deals/src/Crm.Deals.Api/

RUN dotnet restore services/Crm.Deals/src/Crm.Deals.Api/Crm.Deals.Api.csproj

COPY shared/ shared/
COPY services/Crm.Deals/ services/Crm.Deals/

RUN dotnet publish services/Crm.Deals/src/Crm.Deals.Api/Crm.Deals.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Deals.Api.dll"]
