# Project Context

- **Owner:** Zack Way
- **Project:** AI Policy Engine — APIM Policy Engine management UI for AI workloads, implementing AAA (Authentication, Authorization, Accounting) for API management. Built for teams who need bill-back reporting, runover tracking, token utilization, and audit capabilities. Telecom/RADIUS heritage.
- **Stack:** .NET 9 API (Chargeback.Api) with Aspire orchestration (Chargeback.AppHost), React frontend (chargeback-ui), Azure Managed Redis (caching), CosmosDB (long-term trace/audit storage), Azure API Management (policy enforcement), Bicep (infrastructure)
- **Created:** 2026-03-31

## Key Files

- `infra/` — Azure Bicep templates (my primary workspace)
- `policies/` — APIM policy definitions
- `src/Chargeback.AppHost/` — Aspire orchestration
- `src/Dockerfile` — Container build
- `scripts/` — Deployment and utility scripts

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-31 — Phase 0 Complete: Backend Storage Architecture Established

**Phase 0 Status:** ✅ COMPLETE (Freamon + Bunk)

The backend storage architecture has been refactored from Redis-only to a durable CosmosDB source-of-truth pattern with Redis as a write-through cache. Infrastructure implications:

**What Sydnor Needs to Know:**
- **New CosmosDB Container:** `configuration` container added to `ConfigurationContainerInitializer`. Stores plans, clients, pricing, usage policies, and future routing policies. Partitioned by `/id` (document ID).
- **Redis Remains Caching Layer:** All reads still go through Redis first. Write-through cache means writes hit Cosmos first, then Redis is updated.
- **Startup Services:** New services on startup: `RedisToCosmossMigrationService` (one-time migration of existing Redis data) and `CacheWarmingService` (populate Redis from Cosmos). Both are idempotent.
- **No New Azure Resources:** Existing Cosmos + Redis still sufficient. No changes to Bicep or Azure resource provisioning needed.
- **Container Initialization:** `ConfigurationContainerInitializer.cs` now creates `configuration` container with proper schema initialization.
- **Deployment Impact:** Minimal. Startup is slightly slower due to cache warming, but no downtime required. Redis data is automatically migrated on first startup.

**For Phase 1 Onwards:**
- Model Routing will add `routing-policies` to the `configuration` container.
- Multiplier Pricing will extend existing `pricing` and `plans` documents with new fields.
- All future configuration entities will use the same repository pattern + caching layer.

**Test Results:** 129/129 tests pass (36 new Phase 5 tests for repositories/migration/warmup).
