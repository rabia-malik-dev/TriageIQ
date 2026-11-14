FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/TriageIQ.Domain/TriageIQ.Domain.csproj TriageIQ.Domain/
COPY src/TriageIQ.Application/TriageIQ.Application.csproj TriageIQ.Application/
COPY src/TriageIQ.Infrastructure/TriageIQ.Infrastructure.csproj TriageIQ.Infrastructure/
COPY src/TriageIQ.Api/TriageIQ.Api.csproj TriageIQ.Api/

RUN dotnet restore TriageIQ.Api/TriageIQ.Api.csproj

COPY src/TriageIQ.Domain/ TriageIQ.Domain/
COPY src/TriageIQ.Application/ TriageIQ.Application/
COPY src/TriageIQ.Infrastructure/ TriageIQ.Infrastructure/
COPY src/TriageIQ.Api/ TriageIQ.Api/

RUN dotnet publish TriageIQ.Api/TriageIQ.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TriageIQ.Api.dll"]