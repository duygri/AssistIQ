using System.Net;
using System.Net.Http.Json;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Knowledge;
using AssistIQ.Domain.Knowledge;
using FluentAssertions;

namespace AssistIQ.Tests.Api;

public sealed class KnowledgeDocumentApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_AsAdmin_ShouldCreateReadyDocument()
    {
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "admin@assistiq.local", "Admin123!");

        var response = await client.PostAsJsonAsync("/api/knowledge-documents", new RegisterKnowledgeDocumentRequest(
            "refunds.md",
            "text/markdown",
            512,
            "Refund requests are reviewed within two business days."));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<KnowledgeDocumentDto>();
        body.Should().NotBeNull();
        body!.Status.Should().Be(KnowledgeDocumentStatus.Ready);
        body.ProviderFileId.Should().StartWith("file_");
    }

    [Fact]
    public async Task Register_AsSupportAgent_ShouldReturnForbidden()
    {
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");

        var response = await client.PostAsJsonAsync("/api/knowledge-documents", new RegisterKnowledgeDocumentRequest(
            "refunds.md",
            "text/markdown",
            512,
            "Refund requests are reviewed within two business days."));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", login!.Token);
    }
}
