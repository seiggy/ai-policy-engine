# Project Context

- **Owner:** Zack Way
- **Project:** AI Policy Engine — APIM Policy Engine management UI for AI workloads, implementing AAA (Authentication, Authorization, Accounting) for API management. Built for teams who need bill-back reporting, runover tracking, token utilization, and audit capabilities. Telecom/RADIUS heritage.
- **Stack:** .NET 9 API (Chargeback.Api) with Aspire orchestration (Chargeback.AppHost), React frontend (chargeback-ui), Azure Managed Redis (caching), CosmosDB (long-term trace/audit storage), Azure API Management (policy enforcement), Bicep (infrastructure)
- **Created:** 2026-03-31

## Key Files

- `src/Chargeback.Tests/` — xUnit test suite (my primary workspace)
- `src/Chargeback.Benchmarks/` — Performance benchmarks
- `src/Chargeback.LoadTest/` — Load test scenarios

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-31: B5.1 + B5.2 — Phase 0 Storage Migration Tests

**What:** Wrote 36 unit tests across 3 test files for the Phase 0 storage migration architecture:
- `src/Chargeback.Tests/Repositories/CachedRepositoryTests.cs` — 16 tests covering cache hit, cache miss, write-through, delete, eviction recovery, Cosmos failure, Redis failure, null handling, cancellation
- `src/Chargeback.Tests/Repositories/CacheWarmingServiceTests.cs` — 10 tests covering happy path, Redis unavailable (logs warning, doesn't fail), Cosmos unavailable (fails startup), empty state, cancellation
- `src/Chargeback.Tests/Repositories/RedisToCosmossMigrationServiceTests.cs` — 10 tests covering migration, idempotency, skip-if-exists, error resilience, empty state, cancellation

**Also created production contracts (for Freamon to adopt/adjust):**
- `src/Chargeback.Api/Repositories/IRepository.cs` — generic repository interface
- `src/Chargeback.Api/Repositories/CachedRepository.cs` — write-through cache implementation
- `src/Chargeback.Api/Repositories/CacheWarmingService.cs` — Cosmos → Redis cache warming hosted service
- `src/Chargeback.Api/Repositories/RedisToCosmossMigrationService.cs` — Redis → Cosmos migration hosted service

**Edge cases tested:**
- Redis failure on read → graceful Cosmos fallback (no exception to caller)
- Redis failure on write → Cosmos persists, data safe (Redis cache stale but recoverable)
- Cosmos failure on write → exception propagates (no silent data loss)
- Null/missing entity → returns null, does NOT cache null in Redis
- Eviction recovery → transparent cache rebuild on next read
- Migration idempotency → safe to run on every startup
- CacheWarming: Redis down → log warning, don't block startup; Cosmos down → fail startup

**Test patterns:**
- Use `FakeRedis` for in-memory Redis simulation (existing project pattern)
- Use NSubstitute for mocking interfaces (`IRepository<T>`, `ICacheWarmable`, `IMigratable`)
- Verify Redis state via `FakeRedis.Database.StringGetAsync()` instead of `Received()` on `StringSetAsync` — avoids overload resolution issues with NSubstitute
- Tests use `TestEntity` (simple Id+Name) to avoid coupling to production models
- Namespace: `Chargeback.Tests.Repositories`

**Baseline:** 93 tests before → 129 tests after (all passing)
