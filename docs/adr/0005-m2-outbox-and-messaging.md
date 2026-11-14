# ADR-0005: M2 outbox and Service Bus messaging

- **Status:** Accepted
- **Date:** 2026-11-14

## Context

Tickets must be durable before messaging. Publishing directly from the HTTP request risks message loss when the broker is down.

## Decision

On new ticket insert, write an `OutboxMessages` row in the **same database transaction** as the ticket. The API runs `OutboxDispatchHostedService` to publish `TicketReceived` JSON to queue `tickets.incoming`. The worker consumes messages and sets ticket status to `QueuedForTriage`. Service Bus emulator `MaxDeliveryCount` is 5 for dead-letter behavior.

## Consequences

Integration tests use `InMemoryMessageBus` and manual `IOutboxDispatcher` calls. Real Docker flows verify worker logs and SQL status. LLM triage remains M3.