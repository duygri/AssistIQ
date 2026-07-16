namespace AssistIQ.Api.Security;

public sealed class RequestInputSecurityOptions
{
    public const string SectionName = "RequestSecurity";
    public const long DefaultMaxRequestBodySizeBytes = 256 * 1024;

    public long MaxRequestBodySizeBytes { get; set; } = DefaultMaxRequestBodySizeBytes;
}
