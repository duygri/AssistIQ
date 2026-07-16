# Rate Limiting B Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add zero-cost rate limits to AssistIQ's login and AI draft generation endpoints.

**Architecture:** A focused API security module creates fixed-window partitions and the standardized 429 response. Named endpoint policies are attached with MVC attributes and run after authentication.

**Tech Stack:** ASP.NET Core 10 rate limiting, C# 14, xUnit, FluentAssertions

---

### Task 1: Lock policy behavior with tests

**Files:**
- Create: `tests/AssistIQ.Tests/Api/ApiRateLimitPoliciesTests.cs`
- Create: `tests/AssistIQ.Tests/Api/RateLimitingApiTests.cs`

- [x] Test login permits five requests and rejects the sixth.
- [x] Test AI draft partitions by authenticated user ID.
- [x] Test the standardized 429 response and retry header.
- [x] Add a PostgreSQL integration test for login throttling.
- [x] Run focused tests and confirm the missing implementation fails.

### Task 2: Implement named rate-limit policies

**Files:**
- Create: `src/AssistIQ.Api/Security/ApiRateLimitPolicies.cs`
- Create: `src/AssistIQ.Api/Security/RateLimitingServiceCollectionExtensions.cs`
- Modify: `src/AssistIQ.Api/Program.cs`
- Modify: `src/AssistIQ.Api/Controllers/AuthController.cs`
- Modify: `src/AssistIQ.Api/Controllers/DraftsController.cs`
- Modify: `src/AssistIQ.Application/Common/ErrorCodes.cs`

- [x] Register login and AI draft fixed-window policies.
- [x] Add middleware after authentication/authorization context is available.
- [x] Attach policies only to the intended actions.
- [x] Return the safe 429 contract with `Retry-After`.

### Task 3: Verify and publish

- [x] Run non-Docker tests and Release build locally.
- [x] Re-run secret and raw-exception scans.
- [ ] Commit and push the milestone.
- [ ] Confirm the full PostgreSQL integration suite passes in GitHub Actions.
