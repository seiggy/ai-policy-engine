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
