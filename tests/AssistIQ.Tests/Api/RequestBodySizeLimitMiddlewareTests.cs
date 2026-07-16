using System.Text.Json;
using AssistIQ.Api.Security;
using AssistIQ.Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AssistIQ.Tests.Api;

public sealed class RequestBodySizeLimitMiddlewareTests
{
    [Fact]
    public void Options_ShouldDefaultTo256KiB()
    {
        new RequestInputSecurityOptions().MaxRequestBodySizeBytes
            .Should().Be(256 * 1024);
    }

    [Fact]
    public async Task InvokeAsync_WithBodyAtLimit_ShouldCallNextDelegate()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext(RequestInputSecurityOptions.DefaultMaxRequestBodySizeBytes);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WithBodyOneByteOverLimit_ShouldRejectWithoutCallingNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateContext(RequestInputSecurityOptions.DefaultMaxRequestBodySizeBytes + 1);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        body.RootElement.GetProperty("errorCode").GetString().Should().Be(ErrorCodes.RequestTooLarge);
        body.RootElement.GetProperty("correlationId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvokeAsync_WithConfiguredLimit_ShouldUseConfiguredValue()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            maxBodySizeBytes: 1_024);
        var context = CreateContext(contentLength: 1_025);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
    }

    private static RequestBodySizeLimitMiddleware CreateMiddleware(
        RequestDelegate next,
        long maxBodySizeBytes = RequestInputSecurityOptions.DefaultMaxRequestBodySizeBytes)
    {
        return new RequestBodySizeLimitMiddleware(
            next,
            Options.Create(new RequestInputSecurityOptions
            {
                MaxRequestBodySizeBytes = maxBodySizeBytes
            }));
    }

    private static DefaultHttpContext CreateContext(long contentLength)
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "request-limit-test"
        };
        context.Request.ContentLength = contentLength;
        context.Response.Body = new MemoryStream();
        return context;
    }
}
