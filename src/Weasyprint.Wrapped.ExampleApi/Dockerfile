FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as build

WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -o /app/published-app

FROM berthertogen/aspnet-runtime-6-weayprint
ENV ASPNETCORE_ENVIRONMENT Development

WORKDIR /app
COPY --from=build /app/published-app /app

EXPOSE 80

ENTRYPOINT [ "dotnet", "Weasyprint.Wrapped.ExampleApi.dll" ]
