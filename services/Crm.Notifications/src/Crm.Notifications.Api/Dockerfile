FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Notifications/src/Crm.Notifications.Domain/Crm.Notifications.Domain.csproj services/Crm.Notifications/src/Crm.Notifications.Domain/
COPY services/Crm.Notifications/src/Crm.Notifications.Application/Crm.Notifications.Application.csproj services/Crm.Notifications/src/Crm.Notifications.Application/
COPY services/Crm.Notifications/src/Crm.Notifications.Infrastructure/Crm.Notifications.Infrastructure.csproj services/Crm.Notifications/src/Crm.Notifications.Infrastructure/
COPY services/Crm.Notifications/src/Crm.Notifications.Api/Crm.Notifications.Api.csproj services/Crm.Notifications/src/Crm.Notifications.Api/

RUN dotnet restore services/Crm.Notifications/src/Crm.Notifications.Api/Crm.Notifications.Api.csproj

COPY shared/ shared/
COPY services/Crm.Notifications/ services/Crm.Notifications/

RUN dotnet publish services/Crm.Notifications/src/Crm.Notifications.Api/Crm.Notifications.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Notifications.Api.dll"]
