# Request and Input Hardening C Design

## Goal

Reject oversized, malformed, and unsupported API requests before they reach AssistIQ business logic or AI adapters, while returning controlled error responses that do not echo user input.

## Decisions

- Limit every request body to 256 KiB. Configure the Kestrel limit and enforce the declared `Content-Length` in application middleware so the behavior is deterministic in integration tests and remains protected when Kestrel is the edge server.
- Keep the existing controller-based API and `[ApiController]` validation pipeline. Use built-in data-annotation validation rather than adding a third-party validation package.
- Apply these request boundaries:
  - login email: required, valid email, at most 320 characters;
  - login password: required, at most 256 characters;
  - ticket question: required, at most 4,000 characters;
  - customer name: optional, at most 160 characters;
  - customer email: optional, valid email, at most 320 characters;
  - draft instructions: optional, at most 1,000 characters;
  - edited draft answer: required, at most 8,000 characters;
  - knowledge filename: required, at most 260 characters;
  - knowledge content type: required, at most 120 characters;
  - knowledge size: greater than zero and at most 5 MiB;
  - knowledge text: required, at most 20,000 characters.
- Require `application/json` for controller actions that bind request bodies. Requests using another media type return HTTP 415 before the action executes.
- Replace automatic model-validation details with a stable HTTP 400 response containing only `validation_failed`, a controlled message, and `correlationId`. Do not return rejected field values, attempted values, exception details, or request bodies.
- Oversized requests return HTTP 413 with `request_too_large`, a controlled message, and `correlationId` when the application can produce the response.
- Preserve application/domain validation as defense in depth. API validation does not replace business rules such as supported knowledge-document formats.

## Components

- `RequestInputSecurityOptions` owns the 256 KiB default and configuration binding.
- `RequestBodySizeLimitMiddleware` rejects a declared body length above the configured limit and applies the server feature limit before request-body reads begin.
- MVC configuration owns the safe invalid-model-state response.
- Request DTO annotations define transport-level string, email, and numeric boundaries.
- Body-binding controller actions explicitly declare JSON input.

## Error Flow

1. Request-size middleware rejects a declared oversized payload with HTTP 413.
2. MVC rejects unsupported media types with HTTP 415.
3. Model binding and data annotations reject malformed or out-of-range JSON with the safe HTTP 400 contract.
4. Valid transport input reaches application services, where existing business rules still apply.

## Out of Scope

- File uploads or multipart form data.
- HTML sanitization; AssistIQ stores and returns plain API text and does not render it as HTML.
- JWT lifetime, refresh tokens, revocation, CORS, or proxy trust configuration.
- Per-endpoint body-size exceptions.
- Replacing existing service/domain validation.

## Verification

- Boundary values are accepted and values above each boundary are rejected.
- Invalid email and missing required values return the safe 400 contract.
- A body larger than 256 KiB returns 413 before controller or service execution.
- Non-JSON body requests return 415.
- Validation and size responses do not contain submitted secrets or oversized content.
- Existing unit and PostgreSQL integration tests continue to pass.
