# ADR-0002: Target framework .NET 10

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

The build spec calls for the latest LTS .NET with nullable reference types and warnings as errors. The developer machine ships with the .NET 10 SDK.

## Decision

Standardise all projects on `net10.0` via `Directory.Build.props`. CI uses `10.0.x`; Docker images use the official `mcr.microsoft.com/dotnet/*:10.0` tags.

## Consequences

Package versions align with ASP.NET Core 10. GitHub Actions must install the 10.0 SDK. If portability to .NET 8 is required later, we can retarget in a dedicated change.