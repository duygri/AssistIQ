# AssistIQ - SaaS Support Copilot API

[![CI](https://github.com/duygri/AssistIQ/actions/workflows/ci.yml/badge.svg)](https://github.com/duygri/AssistIQ/actions/workflows/ci.yml)

AssistIQ is a portfolio backend for a support-team AI copilot. It is built to show more than CRUD: JWT auth, RBAC, EF Core/PostgreSQL, audit logs, usage/cost tracking, deterministic fake AI infrastructure, and a ticket-to-draft workflow with citations.

## Tech Stack

- ASP.NET Core Web API on .NET 10
- EF Core with PostgreSQL
- JWT authentication and policy-based authorization
- xUnit, WebApplicationFactory, SQLite integration tests
- OpenAPI in development

## Architecture Highlights

- `AssistIQ.Domain`: entity state and business rules such as draft citation gating.
- `AssistIQ.Application`: DTOs, use-case services, stable error codes, and provider-neutral AI boundaries.
- `AssistIQ.Infrastructure`: EF Core persistence, repositories, fake AI/retrieval/indexing adapters, JWT, audit, usage recording, and seed data.
- `AssistIQ.Api`: controllers, JWT bearer auth, and policy-based authorization.

## V1 Scope

- Admin and Support Agent login
- Admin knowledge document registration and disable workflow
- Support Agent ticket creation
- AI draft generation from ready knowledge documents
- Citation gate: drafts without citations cannot be sent
- Draft editing and sending
- Audit log and usage log admin APIs

Out of scope for V1: real OpenAI calls, real file upload, multi-tenant vector stores, billing, mobile app, and real support inbox integrations.

## Local Setup

Requirements:

- .NET SDK 10
- PostgreSQL running locally

Default connection string:

```text
Host=localhost;Port=5432;Database=assistiq;Username=postgres;Password=postgres
```

Apply migrations:

```powershell
dotnet ef database update --project src\AssistIQ.Infrastructure --startup-project src\AssistIQ.Api
```

Run the API:

```powershell
dotnet run --project src\AssistIQ.Api
```

OpenAPI is available in Development at:

```text
/openapi/v1.json
```

## Docker Demo Setup

For a one-command demo with PostgreSQL, migrations, and demo users:

```powershell
docker compose up --build
```

Then open:

```text
http://localhost:5255/health
http://localhost:5255/openapi/v1.json
```

The Docker profile sets:

- `ApplyMigrationsOnStartup=true`
- `SeedDemoDataOnStartup=true`
- PostgreSQL at `localhost:5432`

Stop the stack:

```powershell
docker compose down
```

Remove the database volume:

```powershell
docker compose down -v
```

## Demo Users

Demo data seeding is implemented but disabled by default to avoid startup failures when PostgreSQL is not running. Enable it with:

```json
"SeedDemoDataOnStartup": true
```

Seeded accounts:

- Admin: `admin@assistiq.local` / `Admin123!`
- Support Agent: `agent@assistiq.local` / `Agent123!`

## Core Demo Flow

1. Login as Admin.
2. Register a knowledge document through `POST /api/knowledge-documents`.
3. Login as Support Agent.
4. Create a ticket through `POST /api/tickets`.
5. Generate a draft through `POST /api/tickets/{id}/drafts/generate`.
6. Send the draft through `POST /api/drafts/{id}/send`.
7. Login as Admin and inspect `GET /api/audit-logs` and `GET /api/usage-logs`.

The request collection in `src/AssistIQ.Api/AssistIQ.Api.http` follows this flow.

## API Surface

| Area | Endpoint | Access |
| --- | --- | --- |
| Auth | `POST /api/auth/login` | Public |
| Auth | `GET /api/auth/me` | Authenticated |
| Knowledge | `GET /api/knowledge-documents` | Admin |
| Knowledge | `POST /api/knowledge-documents` | Admin |
| Knowledge | `POST /api/knowledge-documents/{id}/disable` | Admin |
| Tickets | `POST /api/tickets` | Admin, Support Agent |
| Tickets | `GET /api/tickets` | Admin sees all, Support Agent sees own |
| Tickets | `GET /api/tickets/{id}` | Admin or owner |
| Drafts | `POST /api/tickets/{id}/drafts/generate` | Admin or owner |
| Drafts | `PATCH /api/drafts/{id}` | Admin or owner |
| Drafts | `POST /api/drafts/{id}/send` | Admin or owner |
| Admin Logs | `GET /api/audit-logs` | Admin |
| Admin Logs | `GET /api/usage-logs` | Admin |

## Verification

```powershell
dotnet build AssistIQ.slnx
dotnet test AssistIQ.slnx
dotnet list AssistIQ.slnx package --vulnerable --include-transitive
```
