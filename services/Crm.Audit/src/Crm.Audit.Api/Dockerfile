FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Audit/src/Crm.Audit.Domain/Crm.Audit.Domain.csproj services/Crm.Audit/src/Crm.Audit.Domain/
COPY services/Crm.Audit/src/Crm.Audit.Application/Crm.Audit.Application.csproj services/Crm.Audit/src/Crm.Audit.Application/
COPY services/Crm.Audit/src/Crm.Audit.Infrastructure/Crm.Audit.Infrastructure.csproj services/Crm.Audit/src/Crm.Audit.Infrastructure/
COPY services/Crm.Audit/src/Crm.Audit.Api/Crm.Audit.Api.csproj services/Crm.Audit/src/Crm.Audit.Api/

RUN dotnet restore services/Crm.Audit/src/Crm.Audit.Api/Crm.Audit.Api.csproj

COPY shared/ shared/
COPY services/Crm.Audit/ services/Crm.Audit/

RUN dotnet publish services/Crm.Audit/src/Crm.Audit.Api/Crm.Audit.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Audit.Api.dll"]
