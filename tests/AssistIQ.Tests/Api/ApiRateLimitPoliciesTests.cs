using System.Net;
using System.Security.Claims;
using System.Text.Json;
using AssistIQ.Api.Security;
using AssistIQ.Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AssistIQ.Tests.Api;

public sealed class ApiRateLimitPoliciesTests
{
    [Fact]
    public void AddAssistIQRateLimiting_ShouldResolveFinalConfigurationValues()
    {
        var configuration = new ConfigurationManager();
        configuration["RateLimiting:LoginPermitLimit"] = "5";
        configuration["RateLimiting:AiDraftPermitLimit"] = "10";
        var services = new ServiceCollection();
        services.AddAssistIQRateLimiting(configuration);

        configuration["RateLimiting:LoginPermitLimit"] = "1000";
        configuration["RateLimiting:AiDraftPermitLimit"] = "1000";
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AssistIQRateLimitingOptions>>().Value;
        options.LoginPermitLimit.Should().Be(1000);
        options.AiDraftPermitLimit.Should().Be(1000);
    }

    [Fact]
    public void CreateLoginPartition_ShouldPermitFiveRequestsAndRejectSixth()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.10");
        var partition = ApiRateLimitPolicies.CreateLoginPartition(context);
        using var limiter = partition.Factory(partition.PartitionKey);

        for (var request = 0; request < ApiRateLimitPolicies.LoginPermitLimit; request++)
        {
            using var lease = limiter.AttemptAcquire();
            lease.IsAcquired.Should().BeTrue();
        }

        using var rejectedLease = limiter.AttemptAcquire();
        rejectedLease.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public void CreateLoginPartition_WithConfiguredLimit_ShouldUseConfiguredValue()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.11");
        var partition = ApiRateLimitPolicies.CreateLoginPartition(context, permitLimit: 2);
        using var limiter = partition.Factory(partition.PartitionKey);

        using var firstLease = limiter.AttemptAcquire();
        using var secondLease = limiter.AttemptAcquire();
        using var rejectedLease = limiter.AttemptAcquire();

        firstLease.IsAcquired.Should().BeTrue();
        secondLease.IsAcquired.Should().BeTrue();
        rejectedLease.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public void CreateAiDraftPartition_ShouldIsolateAuthenticatedUsers()
    {
        var firstUserId = Guid.NewGuid();
        var firstUser = CreateAuthenticatedContext(firstUserId);
        var sameFirstUser = CreateAuthenticatedContext(firstUserId);
        var secondUser = CreateAuthenticatedContext(Guid.NewGuid());

        var firstPartition = ApiRateLimitPolicies.CreateAiDraftPartition(firstUser);
        var sameFirstPartition = ApiRateLimitPolicies.CreateAiDraftPartition(sameFirstUser);
        var secondPartition = ApiRateLimitPolicies.CreateAiDraftPartition(secondUser);

        firstPartition.PartitionKey.Should().Be(sameFirstPartition.PartitionKey);
        firstPartition.PartitionKey.Should().NotBe(secondPartition.PartitionKey);
    }

    [Fact]
    public void CreateAiDraftPartition_ShouldPermitTenRequestsAndRejectEleventh()
    {
        var context = CreateAuthenticatedContext(Guid.NewGuid());
        var partition = ApiRateLimitPolicies.CreateAiDraftPartition(context);
        using var limiter = partition.Factory(partition.PartitionKey);

        for (var request = 0; request < ApiRateLimitPolicies.AiDraftPermitLimit; request++)
        {
            using var lease = limiter.AttemptAcquire();
            lease.IsAcquired.Should().BeTrue();
        }

        using var rejectedLease = limiter.AttemptAcquire();
        rejectedLease.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public async Task WriteRejectedResponseAsync_ShouldReturnSafeContractAndRetryAfter()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-rate-limit-test"
        };
        context.Response.Body = new MemoryStream();

        await ApiRateLimitPolicies.WriteRejectedResponseAsync(
            context,
            TimeSpan.FromSeconds(12.2),
            CancellationToken.None);

        context.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        context.Response.Headers.RetryAfter.ToString().Should().Be("13");
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        body.RootElement.GetProperty("errorCode").GetString().Should().Be(ErrorCodes.RateLimitExceeded);
        body.RootElement.GetProperty("message").GetString().Should().Be("Too many requests. Try again later.");
        body.RootElement.GetProperty("correlationId").GetString().Should().Be("trace-rate-limit-test");
    }

    private static DefaultHttpContext CreateAuthenticatedContext(Guid userId)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "Test"));
        return context;
    }
}
