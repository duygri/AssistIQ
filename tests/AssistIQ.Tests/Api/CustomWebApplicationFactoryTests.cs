using FluentAssertions;

namespace AssistIQ.Tests.Api;

public sealed class CustomWebApplicationFactoryTests
{
    [Fact]
    public void Fixture_ShouldExposeOnlyOnePublicConstructor()
    {
        typeof(CustomWebApplicationFactory).GetConstructors().Should().ContainSingle();
    }
}
