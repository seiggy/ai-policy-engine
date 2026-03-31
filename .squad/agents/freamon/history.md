# Project Context

- **Owner:** Zack Way
- **Project:** AI Policy Engine — APIM Policy Engine management UI for AI workloads, implementing AAA (Authentication, Authorization, Accounting) for API management. Built for teams who need bill-back reporting, runover tracking, token utilization, and audit capabilities. Telecom/RADIUS heritage.
- **Stack:** .NET 9 API (Chargeback.Api) with Aspire orchestration (Chargeback.AppHost), React frontend (chargeback-ui), Azure Managed Redis (caching), CosmosDB (long-term trace/audit storage), Azure API Management (policy enforcement), Bicep (infrastructure)
- **Created:** 2026-03-31

## Key Files

- `src/Chargeback.Api/` — .NET backend API (my primary workspace)
- `src/Chargeback.AppHost/` — Aspire orchestration
- `src/Chargeback.ServiceDefaults/` — Shared service configuration
- `src/Directory.Packages.props` — Central package management

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### Phase 0 — Storage Migration (2026-03-31)

**What was done:** Implemented full Phase 0 from architecture-v2. CosmosDB is now source of truth for all configuration data (plans, clients, pricing, usage policy). Redis is cache-only with write-through pattern.

**Key files created:**
- `Services/IRepository.cs` — generic repository interface (`IRepository<T>`)
- `Services/CosmosRepositoryBase.cs` — shared Cosmos CRUD base class
- `Services/CosmosPlanRepository.cs`, `CosmosClientRepository.cs`, `CosmosPricingRepository.cs`, `CosmosUsagePolicyRepository.cs` — concrete Cosmos repos
- `Services/CachedRepository.cs` — write-through Redis cache decorator
- `Services/ConfigurationContainerProvider.cs` — Cosmos "configuration" container initialization
- `Services/RedisToCosmossMigrationService.cs` — Redis→Cosmos data migration (IHostedService)
- `Services/CacheWarmingService.cs` — Cosmos→Redis cache warming (IHostedService)

**Key files modified:**
- `Models/PlanData.cs`, `ClientPlanAssignment.cs`, `ModelPricing.cs`, `UsagePolicySettings.cs` — added `Id` and `PartitionKey` for Cosmos
- `Services/IUsagePolicyStore.cs` + `UsagePolicyStore.cs` — refactored to use `IRepository<UsagePolicySettings>` internally
- `Services/LogDataService.cs` — updated for new `IUsagePolicyStore` interface
- `Program.cs` — full DI wiring: Cosmos repos → CachedRepository wrappers → hosted services
- All 6 endpoint files refactored to use `IRepository<T>` instead of direct Redis

**Patterns established:**
- Single Cosmos container "configuration" with partition key `/partitionKey` for all config entities
- Partition values: "plan", "client", "pricing", "settings"
- Write path: endpoint → `IRepository<T>` → `CachedRepository.UpsertAsync` → Cosmos first → Redis cache
- Read path: endpoint → `IRepository<T>` → `CachedRepository.GetAsync` → Redis first → Cosmos fallback
- Startup order: `RedisToCosmossMigrationService` → `CacheWarmingService` → app ready
- Ephemeral data (rate limits, logs, traces, locks) stays Redis-only
- Test fixture uses `RedisBackedRepository<T>` to preserve FakeRedis seeding patterns

**Decisions:**
- `GetAllAsync` always queries Cosmos (source of truth for listings), not Redis scan
- Repository classes made `public` (not internal) for test fixture accessibility
- Corrupted Redis data returns null from repository (treated as "not found"), not 500
