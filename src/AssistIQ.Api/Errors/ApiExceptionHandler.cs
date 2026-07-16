using System.Diagnostics;
using AssistIQ.Application.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace AssistIQ.Api.Errors;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var statusCode = StatusCodes.Status500InternalServerError;
        var errorCode = ErrorCodes.UnexpectedError;
        var message = "An unexpected error occurred.";

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            errorCode = appException.ErrorCode;
            message = appException.Message;
        }
        else
        {
            logger.LogError(
                "Unhandled API exception of type {ExceptionType}. CorrelationId: {CorrelationId}",
                exception.GetType().FullName,
                correlationId);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            errorCode,
            message,
            correlationId
        }, cancellationToken);

        return true;
    }
}
