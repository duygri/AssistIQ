# Rate Limiting B Design

## Goal

Protect login and AI draft generation from brute force and abuse without adding paid services or external infrastructure.

## Decisions

- Use ASP.NET Core's built-in fixed-window rate limiter.
- Limit login to 5 requests per minute per remote IP address by default.
- Limit AI draft generation to 10 requests per minute per authenticated user ID by default.
- Read permit limits from validated configuration so test hosts can isolate unrelated scenarios without changing production defaults.
- Do not queue excess requests.
- Return HTTP 429 with `rate_limit_exceeded`, a controlled message, `correlationId`, and `Retry-After` when available.
- Apply rate limiting only to login and draft generation. Health checks and read endpoints remain unaffected.
- Do not trust forwarded IP headers until a deployment explicitly configures known proxies.

## Security

Partition keys come from server-observed remote IP addresses and validated JWT claims. Rejection responses contain no request body, credentials, provider payload, or internal exception details.

## Verification

- Real limiter instances enforce the configured permit counts with no queue.
- Different users receive different AI draft partitions.
- Rejection responses follow the safe API error contract.
- PostgreSQL integration tests verify the login endpoint returns 429 after the configured limit.
