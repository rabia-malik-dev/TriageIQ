FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/TriageIQ.Domain/TriageIQ.Domain.csproj TriageIQ.Domain/
COPY src/TriageIQ.Application/TriageIQ.Application.csproj TriageIQ.Application/
COPY src/TriageIQ.Infrastructure/TriageIQ.Infrastructure.csproj TriageIQ.Infrastructure/
COPY src/TriageIQ.Worker/TriageIQ.Worker.csproj TriageIQ.Worker/

RUN dotnet restore TriageIQ.Worker/TriageIQ.Worker.csproj

COPY src/TriageIQ.Domain/ TriageIQ.Domain/
COPY src/TriageIQ.Application/ TriageIQ.Application/
COPY src/TriageIQ.Infrastructure/ TriageIQ.Infrastructure/
COPY src/TriageIQ.Worker/ TriageIQ.Worker/

RUN dotnet publish TriageIQ.Worker/TriageIQ.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TriageIQ.Worker.dll"]