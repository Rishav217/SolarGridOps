# Canonical Planning Index

Purpose
- Keep all key product and architecture decisions in one place.
- Avoid re-reading old chats for routine updates.
- Reduce risk when making small changes later.

Source Of Truth Order
1. This file (entry point)
2. phase1-implementation-blueprint.md
3. db-schema-blueprint-v1.md
4. api-contract-blueprint-v1.md
5. project-requirements.md
6. architecture-decisions.md
7. security-and-roles.md
8. delivery-and-rollout.md
9. tech-preferences-history.md (historical context)

Planning Files
- ./phase1-implementation-blueprint.md
- ./db-schema-blueprint-v1.md
- ./api-contract-blueprint-v1.md
- ./project-requirements.md
- ./architecture-decisions.md
- ./security-and-roles.md
- ./delivery-and-rollout.md
- ./tech-preferences-history.md

Execution Blueprints
- Product build sequence: ./phase1-implementation-blueprint.md
- Database design baseline: ./db-schema-blueprint-v1.md
- API surface baseline: ./api-contract-blueprint-v1.md

Update Rules
- Every confirmed decision must be written to one planning file the same day.
- If a decision affects multiple areas, update all impacted files in one pass.
- Keep notes short, concrete, and implementation-focused.
- Never hardcode folder names or absolute paths in design notes unless required.
- Preserve backward compatibility notes for all future changes.

Change Workflow
1. Confirm decision in discussion.
2. Update the relevant planning file(s).
3. If needed, add one summary bullet in this file under Recent Decisions.
4. Apply implementation changes only after notes are updated.

Recent Decisions
- Project root is C:/Programs/SolarGridOps.
- Local-laptop-first pilot, cloud migration later.
- MSSQL preference.
- Permission-driven access with UI hide + API deny for blocked roles.
- Multi-day installation session tracking required.
- Phase 1 blueprint pack created (implementation, DB, API) pending sample-data refinement.
- Tech type locked: ASP.NET Core Web API (backend) + .NET MAUI (Windows + Android client).
- HelioCore was old folder name; SolarGridOps is the canonical project root with all files verified present.
- Backend and MAUI work was delivered through separate feature branches and merged sequentially into develop.
- MAUI now contains dedicated tabs/pages for Dashboard, Inventory workflow, Installation workflow, and KPI dashboard.
- Local Windows-first MAUI builds now default to Windows target only; mobile target matrix remains opt-in via MSBuild property SolarGridOpsEnableMobileTargets.

Session Log
2026-08-13 | Session: HelioCore → SolarGridOps rename, file migration verified, all planning docs confirmed present.
2026-08-13 | Session: Chat history recovered from HelioCore session. All prior decisions still valid. Next step: start Phase 1 build sequence — Step 1 Foundation (solution setup).
2026-08-18 | Session: Sample data analysed (Durgesh Gaju + panel Excels). DB schema updated to v2. Entity coding started — BaseEntity.cs created.
2026-08-24 | Session: Domain entity set completed and validated. EF Core persistence wired (AppDbContext + SQL Server DI). API and Infrastructure build clean; MAUI Android CLI build blocked by local Android SDK path setup.
2026-08-24 | Session: Initial EF migration created and applied to local MSSQL database SolarGridOps_Dev. dotnet-ef tool updated to 9.0.12.
2026-08-24 | Session: Customer vertical slice implemented end-to-end (Application service + repository + API controller). Endpoints available at /api/v1/customers.
2026-08-24 | Session: Security Core runtime stabilized. Added and applied follow-up migration 20260824074342_SecurityCoreUpdate (RefreshTokens.TokenHash length change). Auth smoke test passed for login, refresh, and capabilities endpoints.
2026-08-30 | Session: Feature branches merged into develop in sequence (backend logging, closure, stock movement, sample data, MAUI auth/inventory/installations/KPI).
2026-08-30 | Session: Commit date timeline normalized across delivered branches to align with staged execution window up to 2026-08-27.
2026-08-30 | Session: MAUI local develop errors reduced by making mobile target matrix opt-in; default build target is Windows to avoid missing Android SDK failures on non-mobile dev machines.

Entity Build Progress
[x] BaseEntity (Domain/Entities)
[x] Enums — RoleType, ProjectPhase, DocumentType
[x] Customer
[x] CustomerDocument
[x] Project
[x] ProjectMilestone
[x] InstallationSession
[x] InstallationEvidence
[x] PanelAssignment
[x] InverterAssignment
[x] InvoiceRecord
[x] PaymentReceipt
[x] User, Role, Permission (Security Core)

Build Progress — Phase 1 Step 1 (Foundation)
[x] Solution file created (SolarGridOps.slnx)
[x] Domain project created (net9.0 classlib)
[x] Application project created (net9.0 classlib)
[x] Infrastructure project created (net9.0 classlib)
[x] Api project created (net9.0 webapi)
[x] Maui project created (net9.0 maui)
[x] All projects added to solution
[x] Project references wired (Application→Domain, Infrastructure→Domain+Application, Api→Application+Infrastructure)
[x] Folder structure inside each project (Entities, Enums, Interfaces etc.)
[x] Config system (appsettings + environment profiles)
[x] Central error handling + logging pipeline
[x] Validation pipeline

Next Step
- Validate merged develop on pilot laptop end-to-end (API + MAUI) against production-like sample workflow.
- Add CI profile to run mobile target matrix with SolarGridOpsEnableMobileTargets=true on agents that have Android/iOS workloads.
