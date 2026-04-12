FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Tasks/src/Crm.Tasks.Domain/Crm.Tasks.Domain.csproj services/Crm.Tasks/src/Crm.Tasks.Domain/
COPY services/Crm.Tasks/src/Crm.Tasks.Application/Crm.Tasks.Application.csproj services/Crm.Tasks/src/Crm.Tasks.Application/
COPY services/Crm.Tasks/src/Crm.Tasks.Infrastructure/Crm.Tasks.Infrastructure.csproj services/Crm.Tasks/src/Crm.Tasks.Infrastructure/
COPY services/Crm.Tasks/src/Crm.Tasks.Api/Crm.Tasks.Api.csproj services/Crm.Tasks/src/Crm.Tasks.Api/

RUN dotnet restore services/Crm.Tasks/src/Crm.Tasks.Api/Crm.Tasks.Api.csproj

COPY shared/ shared/
COPY services/Crm.Tasks/ services/Crm.Tasks/

RUN dotnet publish services/Crm.Tasks/src/Crm.Tasks.Api/Crm.Tasks.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Tasks.Api.dll"]
