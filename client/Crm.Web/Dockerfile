FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/Crm.Shared.Domain/Crm.Shared.Domain.csproj shared/Crm.Shared.Domain/
COPY shared/Crm.Shared.Infrastructure/Crm.Shared.Infrastructure.csproj shared/Crm.Shared.Infrastructure/
COPY shared/Crm.Shared.DTOs/Crm.Shared.DTOs.csproj shared/Crm.Shared.DTOs/
COPY client/Crm.Web/Crm.Web.csproj client/Crm.Web/

RUN dotnet restore client/Crm.Web/Crm.Web.csproj

COPY shared/ shared/
COPY client/ client/

RUN dotnet publish client/Crm.Web/Crm.Web.csproj -c Release -o /app/publish

FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY client/Crm.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
