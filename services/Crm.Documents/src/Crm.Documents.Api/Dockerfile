FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY services/Crm.Documents/src/Crm.Documents.Domain/Crm.Documents.Domain.csproj services/Crm.Documents/src/Crm.Documents.Domain/
COPY services/Crm.Documents/src/Crm.Documents.Application/Crm.Documents.Application.csproj services/Crm.Documents/src/Crm.Documents.Application/
COPY services/Crm.Documents/src/Crm.Documents.Infrastructure/Crm.Documents.Infrastructure.csproj services/Crm.Documents/src/Crm.Documents.Infrastructure/
COPY services/Crm.Documents/src/Crm.Documents.Api/Crm.Documents.Api.csproj services/Crm.Documents/src/Crm.Documents.Api/

RUN dotnet restore services/Crm.Documents/src/Crm.Documents.Api/Crm.Documents.Api.csproj

COPY shared/ shared/
COPY services/Crm.Documents/ services/Crm.Documents/

RUN dotnet publish services/Crm.Documents/src/Crm.Documents.Api/Crm.Documents.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Crm.Documents.Api.dll"]
