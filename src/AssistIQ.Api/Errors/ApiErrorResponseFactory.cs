using System.Diagnostics;
using AssistIQ.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AssistIQ.Api.Errors;

public static class ApiErrorResponseFactory
{
    public static IActionResult CreateValidationError(ActionContext context)
    {
        var correlationId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(new
        {
            errorCode = ErrorCodes.ValidationFailed,
            message = "One or more request fields are invalid.",
            correlationId
        });
    }
}
