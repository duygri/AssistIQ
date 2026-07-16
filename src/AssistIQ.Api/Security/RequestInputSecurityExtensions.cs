namespace AssistIQ.Api.Security;

public static class RequestInputSecurityExtensions
{
    public static void AddAssistIQRequestInputSecurity(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<RequestInputSecurityOptions>()
            .Bind(builder.Configuration.GetSection(RequestInputSecurityOptions.SectionName))
            .Validate(
                options => options.MaxRequestBodySizeBytes > 0,
                "RequestSecurity:MaxRequestBodySizeBytes must be greater than zero.")
            .ValidateOnStart();

        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Limits.MaxRequestBodySize = context.Configuration.GetValue<long?>(
                $"{RequestInputSecurityOptions.SectionName}:MaxRequestBodySizeBytes")
                ?? RequestInputSecurityOptions.DefaultMaxRequestBodySizeBytes;
        });
    }
}
