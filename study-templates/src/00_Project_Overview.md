# Study guide — Project overview (template)

Copy this file into private `study/src/` and expand with practice questions per SPEC section 12.

## Practice question (Level 1)

**Q:** What does the transactional outbox guarantee for ticket creation?

**30-second answer:** Ticket and outbox row commit together; a publisher sends after commit.

**Where in code:** `TicketService.CreateAsync`, `OutboxDispatchHostedService`.