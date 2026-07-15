# PostgreSQL Testcontainers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Run AssistIQ API integration tests against PostgreSQL so test and production database behavior match.

**Architecture:** `CustomWebApplicationFactory` owns one PostgreSQL Testcontainer per xUnit class fixture, replaces the API connection string through in-memory configuration, and resets state by recreating the database from EF Core migrations before seeding demo users. Pure unit tests remain container-free.

**Tech Stack:** .NET 10, ASP.NET Core `WebApplicationFactory`, EF Core/Npgsql, xUnit, Testcontainers for .NET, PostgreSQL 16.

---

### Task 1: Lock the database-provider contract

**Files:**
- Create: `tests/AssistIQ.Tests/Api/DatabaseProviderApiTests.cs`

- [ ] Add a test that resolves `AssistIQDbContext` from the API test host and requires the Npgsql provider.
- [ ] Run the test against the existing SQLite factory and confirm it fails because the provider is SQLite.

### Task 2: Replace SQLite with PostgreSQL Testcontainers

**Files:**
- Modify: `tests/AssistIQ.Tests/AssistIQ.Tests.csproj`
- Modify: `tests/AssistIQ.Tests/Api/CustomWebApplicationFactory.cs`

- [ ] Add `Testcontainers.PostgreSql`; keep SQLite only for container-free application tests.
- [ ] Start a PostgreSQL 16 Alpine container before the API host is created.
- [ ] Override `ConnectionStrings:DefaultConnection` with the container connection string.
- [ ] Reset the database with EF Core migrations, then seed demo users.
- [ ] Run the provider contract test and the full suite in a Docker-capable environment.

### Task 3: Document and verify the workflow

**Files:**
- Modify: `README.md`
- Verify: `.github/workflows/ci.yml`

- [ ] Document Docker as a requirement for API integration tests.
- [ ] Remove the completed Testcontainers item from the roadmap.
- [ ] Build locally, run container-free tests locally, and run the full suite in GitHub Actions.
