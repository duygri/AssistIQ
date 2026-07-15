# AssistIQ - SaaS Support Copilot API

[![CI](https://github.com/duygri/AssistIQ/actions/workflows/ci.yml/badge.svg)](https://github.com/duygri/AssistIQ/actions/workflows/ci.yml)

AssistIQ is a portfolio backend for a support-team AI copilot. It is built to show more than CRUD: JWT auth, RBAC, EF Core/PostgreSQL, audit logs, usage/cost tracking, deterministic fake AI infrastructure, and a ticket-to-draft workflow with citations.

## Tech Stack

- ASP.NET Core Web API on .NET 10
- EF Core with PostgreSQL
- JWT authentication and policy-based authorization
- xUnit, WebApplicationFactory, PostgreSQL integration tests with Testcontainers
- OpenAPI in development

## Architecture Highlights

- `AssistIQ.Domain`: entity state and business rules such as draft citation gating.
- `AssistIQ.Application`: DTOs, use-case services, stable error codes, and provider-neutral AI boundaries.
- `AssistIQ.Infrastructure`: EF Core persistence, repositories, fake AI/retrieval/indexing adapters, JWT, audit, usage recording, and seed data.
- `AssistIQ.Api`: controllers, JWT bearer auth, and policy-based authorization.

```mermaid
flowchart LR
    Client[API client / Swagger / .http] --> Api[AssistIQ.Api]
    Api --> App[AssistIQ.Application]
    App --> Domain[AssistIQ.Domain]
    App --> Infra[AssistIQ.Infrastructure]
    Infra --> Db[(PostgreSQL)]
    Infra --> FakeAi[Deterministic fake AI]
    Infra --> Audit[Audit and usage logs]
```

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

These credentials are intentionally public demo seed data. Production secrets and user credentials should be provided through a secret manager or environment variables, not committed configuration.

## Demo Preview

![AssistIQ demo preview](docs/assets/assistiq-demo-preview.svg)

## Core Demo Flow

1. Login as Admin.
2. Register a knowledge document through `POST /api/knowledge-documents`.
3. Login as Support Agent.
4. Create a ticket through `POST /api/tickets`.
5. Generate a draft through `POST /api/tickets/{id}/drafts/generate`.
6. Send the draft through `POST /api/drafts/{id}/send`.
7. Login as Admin and inspect `GET /api/audit-logs` and `GET /api/usage-logs`.

The request collection in `src/AssistIQ.Api/AssistIQ.Api.http` follows this flow.

## Example Requests and Responses

Login returns a JWT and the current user's role:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "agent@assistiq.local",
  "password": "Agent123!"
}
```

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "11111111-1111-1111-1111-111111111111",
    "email": "agent@assistiq.local",
    "displayName": "Support Agent",
    "role": "SupportAgent"
  }
}
```

Registering a knowledge document makes it available to the deterministic retrieval adapter:

```http
POST /api/knowledge-documents
Authorization: Bearer <admin-jwt>
Content-Type: application/json

{
  "fileName": "billing.md",
  "contentType": "text/markdown",
  "sizeBytes": 512,
  "textContent": "Billing details can be updated from workspace settings."
}
```

```json
{
  "id": "22222222-2222-2222-2222-222222222222",
  "fileName": "billing.md",
  "contentType": "text/markdown",
  "sizeBytes": 512,
  "status": "Ready",
  "providerVectorStoreId": "fake-vector-store",
  "providerFileId": "fake-file-22222222",
  "errorSummary": null,
  "uploadedAt": "2026-07-15T10:00:00Z",
  "indexedAt": "2026-07-15T10:00:00Z",
  "disabledAt": null
}
```

Generating a draft creates a versioned answer with citations. The citation gate is a domain rule: a draft cannot be sent unless it has at least one citation.

```http
POST /api/tickets/33333333-3333-3333-3333-333333333333/drafts/generate
Authorization: Bearer <agent-jwt>
Content-Type: application/json

{
  "instructions": "Use a concise and friendly tone."
}
```

```json
{
  "id": "44444444-4444-4444-4444-444444444444",
  "ticketId": "33333333-3333-3333-3333-333333333333",
  "versionNumber": 1,
  "source": "AiGenerated",
  "status": "Generated",
  "generatedAnswer": "Thanks for reaching out. Based on our support knowledge, how do I update billing details? can be handled using the cited policy.",
  "editedAnswer": null,
  "createdAt": "2026-07-15T10:01:00Z",
  "editedAt": null,
  "sentAt": null,
  "citations": [
    {
      "id": "55555555-5555-5555-5555-555555555555",
      "knowledgeDocumentId": "22222222-2222-2222-2222-222222222222",
      "fileName": "billing.md",
      "providerFileId": "fake-file-22222222",
      "quote": "Relevant support policy excerpt from billing.md.",
      "providerResultId": "fake_result_1",
      "confidence": 0.91
    }
  ]
}
```

If retrieval finds no ready knowledge document, draft generation fails with a stable error code:

```json
{
  "errorCode": "no_ready_knowledge_document",
  "message": "At least one ready knowledge document is required."
}
```

The domain also protects the send operation if a draft reaches it without citations:

```json
{
  "errorCode": "draft_needs_citation_review",
  "message": "Draft cannot be sent without at least one citation."
}
```

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

## Testing Notes

The test suite uses xUnit, WebApplicationFactory, and Testcontainers. API integration tests start an isolated PostgreSQL 16 container, apply the real EF Core migrations, and seed demo users before each test class. This verifies database behavior, constraints, and provider-specific SQL against the same engine used in deployment.

Docker Engine must be running for the complete test suite:

```powershell
dotnet test AssistIQ.slnx
```

Pure domain and application unit tests remain container-free and can be run without Docker:

```powershell
dotnet test AssistIQ.slnx --filter "FullyQualifiedName!~AssistIQ.Tests.Api"
```

GitHub Actions runs the complete PostgreSQL-backed suite on every push and pull request.

Pagination is intentionally deferred in V1 because the seeded demo data is small. For production hardening, list endpoints such as `GET /api/tickets`, `GET /api/knowledge-documents`, `GET /api/audit-logs`, and `GET /api/usage-logs` should accept page/size parameters and return a paged response envelope.

## Roadmap

- Hosted demo link or short screen recording for recruiters who will not run Docker locally.
- Real OpenAI integration behind the existing `IAiDraftService`, `IRetrievalService`, and `IUsageRecorder` boundaries.
- Optional dashboard only if the project is positioned for full-stack roles.

## Verification

```powershell
dotnet build AssistIQ.slnx
dotnet test AssistIQ.slnx
dotnet list AssistIQ.slnx package --vulnerable --include-transitive
```
