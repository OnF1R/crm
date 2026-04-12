FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Identity/src/Crm.Identity.Domain/Crm.Identity.Domain.csproj services/Crm.Identity/src/Crm.Identity.Domain/
COPY services/Crm.Identity/src/Crm.Identity.Application/Crm.Identity.Application.csproj services/Crm.Identity/src/Crm.Identity.Application/
COPY services/Crm.Identity/src/Crm.Identity.Infrastructure/Crm.Identity.Infrastructure.csproj services/Crm.Identity/src/Crm.Identity.Infrastructure/
COPY services/Crm.Identity/src/Crm.Identity.Api/Crm.Identity.Api.csproj services/Crm.Identity/src/Crm.Identity.Api/

RUN dotnet restore services/Crm.Identity/src/Crm.Identity.Api/Crm.Identity.Api.csproj

COPY shared/ shared/
COPY services/Crm.Identity/ services/Crm.Identity/

RUN dotnet publish services/Crm.Identity/src/Crm.Identity.Api/Crm.Identity.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Identity.Api.dll"]
