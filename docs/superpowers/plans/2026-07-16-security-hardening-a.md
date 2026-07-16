# Security Hardening A Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Production fail closed on demo configuration and eliminate raw internal exception leakage.

**Architecture:** A startup validator owns production configuration invariants. A single ASP.NET Core exception handler owns HTTP error serialization, while application services persist stable failure codes only.

**Tech Stack:** ASP.NET Core 10, C# 14, xUnit, FluentAssertions, PostgreSQL/Npgsql

---

### Task 1: Lock the security contract with tests

**Files:**
- Create: `tests/AssistIQ.Tests/Api/ProductionSecurityValidatorTests.cs`
- Create: `tests/AssistIQ.Tests/Api/ApiExceptionHandlerTests.cs`
- Modify: `tests/AssistIQ.Tests/Application/KnowledgeDocumentServiceTests.cs`

- [x] Test Production rejects demo JWT/database credentials and demo seeding.
- [x] Test unexpected exceptions return only a generic error and correlation ID.
- [x] Test indexing failures persist `indexing_failed`, not the exception message.
- [x] Run focused tests and confirm they fail for the intended missing behavior.

### Task 2: Implement fail-closed API security

**Files:**
- Create: `src/AssistIQ.Api/Security/ProductionSecurityValidator.cs`
- Create: `src/AssistIQ.Api/Errors/ApiExceptionHandler.cs`
- Modify: `src/AssistIQ.Api/Program.cs`
- Modify: `src/AssistIQ.Api/Controllers/*.cs`

- [x] Validate Production configuration before the request pipeline starts.
- [x] Register and activate the global exception handler.
- [x] Enable HSTS outside Development.
- [x] Remove controller-local exception mapping.
- [x] Run focused tests and confirm they pass.

### Task 3: Sanitize persisted failures

**Files:**
- Modify: `src/AssistIQ.Application/Drafts/DraftService.cs`
- Modify: `src/AssistIQ.Application/Knowledge/KnowledgeDocumentService.cs`

- [x] Store stable error codes for provider and indexing failures.
- [x] Replace domain exception passthrough with controlled client messages.
- [x] Run all tests available without Docker; PostgreSQL tests remain covered by CI.
- [x] Re-run repository secret and raw-exception scans.
