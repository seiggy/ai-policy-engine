# Project Context

- **Owner:** Zack Way
- **Project:** AI Policy Engine — APIM Policy Engine management UI for AI workloads, implementing AAA (Authentication, Authorization, Accounting) for API management. Built for teams who need bill-back reporting, runover tracking, token utilization, and audit capabilities. Telecom/RADIUS heritage.
- **Stack:** .NET 9 API (Chargeback.Api) with Aspire orchestration (Chargeback.AppHost), React frontend (chargeback-ui), Azure Managed Redis (caching), CosmosDB (long-term trace/audit storage), Azure API Management (policy enforcement), Bicep (infrastructure)
- **Created:** 2026-03-31

## Key Files

- `src/chargeback-ui/` — React frontend (my primary workspace)
- `src/chargeback-ui/package.json` — Frontend dependencies

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-31 — Phase 0 Complete: Backend Storage Architecture Established

**Phase 0 Status:** ✅ COMPLETE (Freamon + Bunk)

The backend storage architecture has been refactored from Redis-only to a durable CosmosDB source-of-truth pattern with Redis as a write-through cache. This is the foundational layer for all upcoming work (routing, pricing, policy enhancements).

**Key Implications for Frontend:**
- **Backend API contracts unchanged** — All endpoint signatures remain the same. The refactoring is internal (storage layer only).
- **Data durability improved** — Configuration data (plans, clients, pricing, routing policies) now survives Redis restarts and evictions.
- **Performance unchanged** — Redis remains the read cache; startup is now slightly slower due to cache warming, but request latency is identical.
- **New Repositories Pattern** — Future frontend changes will interact with the same API endpoints, which now use `IRepository<T>` abstraction instead of direct Redis.

**What Kima Needs to Know:**
- Phase 1 (Model Routing) will add new fields to the precheck response: `routedDeployment` (the actual deployment after routing is applied).
- Future billing UI will need to adapt based on plan configuration (Phase 2–3 multiplier pricing work).
- No frontend code changes required for Phase 0 — backend refactoring only.
- Phase 0 completes the architectural debt fix; Phase 1 onwards adds new features without storage concerns.

**Test Results:** 129/129 tests pass (36 new Phase 5 tests for repositories/migration/warmup).
