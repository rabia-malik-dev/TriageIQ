FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/TriageIQ.Domain/TriageIQ.Domain.csproj src/TriageIQ.Domain/
COPY src/TriageIQ.Application/TriageIQ.Application.csproj src/TriageIQ.Application/
COPY src/TriageIQ.Infrastructure/TriageIQ.Infrastructure.csproj src/TriageIQ.Infrastructure/
COPY src/TriageIQ.Worker/TriageIQ.Worker.csproj src/TriageIQ.Worker/

RUN dotnet restore src/TriageIQ.Worker/TriageIQ.Worker.csproj

COPY src/ src/

RUN dotnet publish src/TriageIQ.Worker/TriageIQ.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TriageIQ.Worker.dll"]