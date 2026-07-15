# GitHub Models Provider Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a real, zero-budget GitHub Models draft provider while keeping the deterministic fake provider as the default and CI-safe fallback.

**Architecture:** A typed `HttpClient` adapter implements the existing `IAiDraftService` boundary and calls GitHub Models chat completions. A focused DI extension selects `Fake` or `GitHubModels` from configuration, with secrets supplied only through environment variables. Retrieval and citation gating remain provider-neutral.

**Tech Stack:** .NET 10, ASP.NET Core dependency injection, `IHttpClientFactory`, GitHub Models REST API, xUnit, FluentAssertions.

---

### Task 1: Lock adapter and provider-selection contracts

**Files:**
- Create: `tests/AssistIQ.Tests/Infrastructure/GitHubModelsAiDraftServiceTests.cs`
- Create: `tests/AssistIQ.Tests/Infrastructure/AiServiceCollectionExtensionsTests.cs`

- [ ] Test request headers, model, prompt context, response usage, and citation mapping.
- [ ] Test that a missing token fails before an HTTP request is sent.
- [ ] Test `Fake`, `GitHubModels`, and unsupported provider selection.
- [ ] Run the focused tests and confirm RED because the adapter and registration extension do not exist.

### Task 2: Implement the GitHub Models provider

**Files:**
- Create: `src/AssistIQ.Infrastructure/Ai/GitHubModelsOptions.cs`
- Create: `src/AssistIQ.Infrastructure/Ai/GitHubModelsAiDraftService.cs`
- Create: `src/AssistIQ.Infrastructure/Ai/AiServiceCollectionExtensions.cs`
- Modify: `src/AssistIQ.Application/Abstractions/IAiDraftService.cs`
- Modify: `src/AssistIQ.Infrastructure/Ai/FakeAiDraftService.cs`
- Modify: `src/AssistIQ.Api/Program.cs`

- [ ] Add provider/model metadata to the AI service boundary.
- [ ] Implement the non-streaming chat-completions request and response mapping.
- [ ] Register the selected provider and a 30-second HTTP timeout.
- [ ] Keep `Fake` as the default and reject unknown provider names at startup.
- [ ] Run focused tests and confirm GREEN.

### Task 3: Make usage logs and documentation provider-aware

**Files:**
- Modify: `src/AssistIQ.Application/Drafts/DraftService.cs`
- Modify: `src/AssistIQ.Domain/Knowledge/KnowledgeDocument.cs`
- Modify: `src/AssistIQ.Infrastructure/Ai/FakeRetrievalService.cs`
- Modify: `src/AssistIQ.Infrastructure/Persistence/Configurations/KnowledgeDocumentConfiguration.cs`
- Create: `src/AssistIQ.Infrastructure/Persistence/Migrations/*_StoreKnowledgeDocumentText.cs`
- Modify: `src/AssistIQ.Api/appsettings.json`
- Modify: `src/AssistIQ.Api/appsettings.Development.json`
- Modify: `README.md`

- [ ] Record the selected provider/model when generation fails.
- [ ] Persist registered document text and return it as grounded retrieval context.
- [ ] Document zero-cost quota behavior, PAT permissions, environment variables, and fake fallback.
- [ ] Run the complete available verification suite and GitHub Actions.
