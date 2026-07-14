using AssistIQ.Application.Abstractions;

namespace AssistIQ.Infrastructure.Services;

public sealed class SystemClock : ISystemClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
