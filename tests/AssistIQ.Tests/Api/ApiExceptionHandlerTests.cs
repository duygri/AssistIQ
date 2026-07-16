using System.Text.Json;
using AssistIQ.Api.Errors;
using AssistIQ.Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssistIQ.Tests.Api;

public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithAppException_ShouldPreserveControlledContract()
    {
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-app-error"
        };
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new AppException(404, ErrorCodes.NotFound, "Ticket was not found."),
            CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        body.RootElement.GetProperty("errorCode").GetString().Should().Be(ErrorCodes.NotFound);
        body.RootElement.GetProperty("message").GetString().Should().Be("Ticket was not found.");
        body.RootElement.GetProperty("correlationId").GetString().Should().Be("trace-app-error");
    }

    [Fact]
    public async Task TryHandleAsync_WithUnexpectedException_ShouldNotExposeInternalDetails()
    {
        const string internalDetails = "password=private; provider payload was rejected";
        var logger = new CapturingLogger<ApiExceptionHandler>();
        var handler = new ApiExceptionHandler(logger);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-security-test"
        };
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException(internalDetails),
            CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        body.RootElement.GetProperty("errorCode").GetString().Should().Be("unexpected_error");
        body.RootElement.GetProperty("message").GetString().Should().Be("An unexpected error occurred.");
        body.RootElement.GetProperty("correlationId").GetString().Should().Be("trace-security-test");
        body.RootElement.ToString().Should().NotContain(internalDetails).And.NotContain("stack");
        logger.Output.Should().NotContain(internalDetails);
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        private readonly List<string> _entries = [];

        public string Output => string.Join(Environment.NewLine, _entries);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _entries.Add(formatter(state, exception));
            if (exception is not null)
            {
                _entries.Add(exception.ToString());
            }
        }
    }
}
