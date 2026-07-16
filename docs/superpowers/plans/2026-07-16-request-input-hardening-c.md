# Request and Input Hardening C Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reject oversized, malformed, and non-JSON API requests through controlled transport-level boundaries before business or AI code runs.

**Architecture:** A small API security module binds and validates a 256 KiB request limit, configures Kestrel, and applies an early middleware limit. MVC data annotations enforce DTO boundaries, while `InvalidModelStateResponseFactory` emits the existing safe error shape and body actions declare JSON input explicitly.

**Tech Stack:** .NET 10, ASP.NET Core controllers, data annotations, Kestrel limits, xUnit, FluentAssertions, WebApplicationFactory, PostgreSQL Testcontainers.

---

### Task 1: Lock the safe validation contract

**Files:**
- Create: `tests/AssistIQ.Tests/Api/RequestInputValidationTests.cs`
- Modify: `src/AssistIQ.Application/Common/ErrorCodes.cs`
- Modify: `src/AssistIQ.Api/Program.cs`
- Create: `src/AssistIQ.Api/Errors/ApiErrorResponseFactory.cs`

- [ ] Write an integration test that posts an invalid login email containing a secret marker.
- [ ] Assert HTTP 400 contains only `validation_failed`, a controlled message, and `correlationId`, and does not echo the marker or validation internals.
- [ ] Run the focused test and confirm it fails against the default MVC validation response.
- [ ] Add data annotations to `LoginRequest` and configure `InvalidModelStateResponseFactory` through a focused response factory.
- [ ] Run the focused test and confirm it passes.

### Task 2: Add DTO boundary validation

**Files:**
- Modify: `src/AssistIQ.Application/Auth/LoginRequest.cs`
- Modify: `src/AssistIQ.Application/Tickets/CreateTicketRequest.cs`
- Modify: `src/AssistIQ.Application/Drafts/GenerateDraftRequest.cs`
- Modify: `src/AssistIQ.Application/Drafts/UpdateDraftRequest.cs`
- Modify: `src/AssistIQ.Application/Knowledge/RegisterKnowledgeDocumentRequest.cs`
- Test: `tests/AssistIQ.Tests/Api/RequestInputValidationTests.cs`

- [ ] Add failing theory tests for each accepted maximum and each value one character above the maximum.
- [ ] Add failing tests for required fields, email shape, and the knowledge-size numeric range.
- [ ] Run the focused tests and verify the expected boundary failures.
- [ ] Add built-in data annotations matching the design limits.
- [ ] Run the focused tests and verify all boundaries pass.

### Task 3: Require JSON input

**Files:**
- Modify: `src/AssistIQ.Api/Controllers/AuthController.cs`
- Modify: `src/AssistIQ.Api/Controllers/TicketsController.cs`
- Modify: `src/AssistIQ.Api/Controllers/DraftsController.cs`
- Modify: `src/AssistIQ.Api/Controllers/KnowledgeDocumentsController.cs`
- Test: `tests/AssistIQ.Tests/Api/RequestInputValidationTests.cs`

- [ ] Add a failing integration test that sends a valid login body as `text/plain`.
- [ ] Assert HTTP 415 and verify the credentials are absent from the response.
- [ ] Add `[Consumes("application/json")]` to every action that binds a body DTO.
- [ ] Run the focused tests and confirm JSON with charset remains accepted while `text/plain` is rejected.

### Task 4: Enforce the 256 KiB request limit

**Files:**
- Create: `src/AssistIQ.Api/Security/RequestInputSecurityOptions.cs`
- Create: `src/AssistIQ.Api/Security/RequestBodySizeLimitMiddleware.cs`
- Create: `src/AssistIQ.Api/Security/RequestInputSecurityExtensions.cs`
- Modify: `src/AssistIQ.Api/Program.cs`
- Modify: `src/AssistIQ.Api/appsettings.json`
- Modify: `src/AssistIQ.Api/appsettings.Development.json`
- Test: `tests/AssistIQ.Tests/Api/RequestBodySizeLimitMiddlewareTests.cs`
- Test: `tests/AssistIQ.Tests/Api/RequestInputValidationTests.cs`

- [ ] Add unit tests for the default/configured limit and middleware pass-through behavior.
- [ ] Add a failing integration test with a declared request body over 256 KiB containing a secret marker.
- [ ] Assert HTTP 413 returns `request_too_large`, `correlationId`, and no marker.
- [ ] Bind validated options, configure Kestrel's `MaxRequestBodySize`, and add early request-size middleware.
- [ ] Run unit and integration tests and verify the next delegate is not called for oversized requests.

### Task 5: Verify and publish

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/plans/2026-07-16-request-input-hardening-c.md`

- [ ] Document request limits and the safe validation contract.
- [ ] Run `dotnet test AssistIQ.slnx --filter "FullyQualifiedName!~AssistIQ.Tests.Api"` with the working local .NET SDK.
- [ ] Run `dotnet build AssistIQ.slnx -c Release --no-restore`.
- [ ] Run `dotnet list AssistIQ.slnx package --vulnerable --include-transitive`.
- [ ] Scan tracked files and Git history for secret patterns and sensitive logging.
- [ ] Inspect `git diff --check` and the complete diff.
- [ ] Commit and push the milestone.
- [ ] Confirm the full PostgreSQL integration suite, vulnerability scan, and Docker build pass in GitHub Actions.
