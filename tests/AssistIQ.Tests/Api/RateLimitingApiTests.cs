using System.Net;
using System.Net.Http.Json;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Common;
using FluentAssertions;

namespace AssistIQ.Tests.Api;

public sealed class RateLimitingApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = new(useProductionRateLimits: true);

    public async Task InitializeAsync()
    {
        await ((IAsyncLifetime)_factory).InitializeAsync();
        await _factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await ((IAsyncLifetime)_factory).DisposeAsync();
    }

    [Fact]
    public async Task Login_AfterFiveAttemptsFromSameIp_ShouldReturnTooManyRequests()
    {
        using var client = _factory.CreateClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await LoginWithBadPasswordAsync(client);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var rejected = await LoginWithBadPasswordAsync(client);

        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.Should().NotBeNull();
        var body = await rejected.Content.ReadFromJsonAsync<RateLimitErrorResponse>();
        body.Should().NotBeNull();
        body!.ErrorCode.Should().Be(ErrorCodes.RateLimitExceeded);
        body.CorrelationId.Should().NotBeNullOrWhiteSpace();
    }

    private static Task<HttpResponseMessage> LoginWithBadPasswordAsync(HttpClient client)
    {
        return client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@assistiq.local",
            "wrong-password"));
    }

    private sealed record RateLimitErrorResponse(string ErrorCode, string Message, string CorrelationId);
}
