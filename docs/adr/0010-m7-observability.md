# ADR-0010: M7 OpenTelemetry and ops

- **Status:** Accepted
- **Date:** 2026-11-14

## Decision

Instrument API and Worker with OpenTelemetry (OTLP when `OpenTelemetry:OtlpEndpoint` is set). Docker Compose `observability` profile runs collector, Jaeger, Prometheus, and Grafana. Admin ops surface: `/api/ops/dlq` with replay/discard and DLQ snapshots recorded at max Service Bus deliveries.

## Consequences

Local traces when profile is enabled; DLQ replay republishes stored payloads to `tickets.incoming`.