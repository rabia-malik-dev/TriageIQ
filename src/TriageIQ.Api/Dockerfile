FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/TriageIQ.Domain/TriageIQ.Domain.csproj src/TriageIQ.Domain/
COPY src/TriageIQ.Application/TriageIQ.Application.csproj src/TriageIQ.Application/
COPY src/TriageIQ.Infrastructure/TriageIQ.Infrastructure.csproj src/TriageIQ.Infrastructure/
COPY src/TriageIQ.Api/TriageIQ.Api.csproj src/TriageIQ.Api/

RUN dotnet restore src/TriageIQ.Api/TriageIQ.Api.csproj

COPY src/ src/

RUN dotnet publish src/TriageIQ.Api/TriageIQ.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TriageIQ.Api.dll"]