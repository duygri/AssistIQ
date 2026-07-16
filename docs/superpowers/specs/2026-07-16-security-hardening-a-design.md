# Security Hardening A Design

## Goal

Prevent AssistIQ from starting in Production with demo credentials and prevent internal exception details from reaching API clients or persisted operational records.

## Decisions

- Keep `Fake` as the default AI provider. This milestone adds no outbound integration.
- Validate security-sensitive configuration before authentication services are created. Production rejects the known demo JWT key, signing keys shorter than 32 UTF-8 bytes, the demo PostgreSQL password, and demo-data seeding.
- Route application and unexpected exceptions through one ASP.NET Core `IExceptionHandler`.
- Return `errorCode`, a controlled message, and `correlationId`. Unexpected exceptions always use a generic message.
- Log unexpected exceptions server-side using structured logging. Never include exception details in the response.
- Persist stable failure codes instead of provider or indexing exception messages.
- Enable HSTS outside Development.

## Compatibility

Existing `AppException` status codes and controlled messages remain part of the API contract. Controller-local exception mapping is removed to avoid divergent error formats.

## Verification

- Production configuration validation fails for demo or weak credentials.
- The global handler does not expose an unexpected exception message or stack trace.
- Failed indexing and AI generation store stable error codes rather than raw exception text.
- Existing API and application tests continue to pass.
