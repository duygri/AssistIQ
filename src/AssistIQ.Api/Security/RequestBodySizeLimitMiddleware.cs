using System.Diagnostics;
using AssistIQ.Application.Common;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AssistIQ.Api.Security;

public sealed class RequestBodySizeLimitMiddleware(
    RequestDelegate next,
    IOptions<RequestInputSecurityOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var maxBodySize = options.Value.MaxRequestBodySizeBytes;
        var sizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();

        if (sizeFeature is { IsReadOnly: false })
        {
            sizeFeature.MaxRequestBodySize = maxBodySize;
        }

        if (context.Request.ContentLength > maxBodySize)
        {
            var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await context.Response.WriteAsJsonAsync(new
            {
                errorCode = ErrorCodes.RequestTooLarge,
                message = "Request body is too large.",
                correlationId
            }, context.RequestAborted);
            return;
        }

        await next(context);
    }
}
