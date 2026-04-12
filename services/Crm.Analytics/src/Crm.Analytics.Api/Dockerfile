FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Analytics/src/Crm.Analytics.Domain/Crm.Analytics.Domain.csproj services/Crm.Analytics/src/Crm.Analytics.Domain/
COPY services/Crm.Analytics/src/Crm.Analytics.Application/Crm.Analytics.Application.csproj services/Crm.Analytics/src/Crm.Analytics.Application/
COPY services/Crm.Analytics/src/Crm.Analytics.Infrastructure/Crm.Analytics.Infrastructure.csproj services/Crm.Analytics/src/Crm.Analytics.Infrastructure/
COPY services/Crm.Analytics/src/Crm.Analytics.Api/Crm.Analytics.Api.csproj services/Crm.Analytics/src/Crm.Analytics.Api/

RUN dotnet restore services/Crm.Analytics/src/Crm.Analytics.Api/Crm.Analytics.Api.csproj

COPY shared/ shared/
COPY services/Crm.Analytics/ services/Crm.Analytics/

RUN dotnet publish services/Crm.Analytics/src/Crm.Analytics.Api/Crm.Analytics.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Analytics.Api.dll"]
