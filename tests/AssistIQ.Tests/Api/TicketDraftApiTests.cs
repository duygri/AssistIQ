using System.Net;
using System.Net.Http.Json;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Drafts;
using AssistIQ.Application.Knowledge;
using AssistIQ.Application.Tickets;
using AssistIQ.Domain.Drafts;
using AssistIQ.Domain.Tickets;
using FluentAssertions;

namespace AssistIQ.Tests.Api;

public sealed class TicketDraftApiTests(CustomWebApplicationFactory factory)
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
    public async Task CreateTicket_WithoutToken_ShouldReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest(
            "How do I update billing details?",
            "Linh",
            "linh@example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTicket_AsSupportAgent_ShouldCreateOpenTicket()
    {
        var client = factory.CreateClient();
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");

        var ticket = await CreateTicketAsync(client);

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.CustomerQuestion.Should().Contain("billing");
    }

    [Fact]
    public async Task GenerateDraft_WithReadyKnowledge_ShouldReturnCitationAndVersionOne()
    {
        var client = factory.CreateClient();
        await SeedKnowledgeAsAdminAsync(client);
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");
        var ticket = await CreateTicketAsync(client);

        var response = await client.PostAsJsonAsync($"/api/tickets/{ticket.Id}/drafts/generate", new GenerateDraftRequest(null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var draft = await response.Content.ReadFromJsonAsync<DraftDto>();
        draft.Should().NotBeNull();
        draft!.Status.Should().Be(DraftStatus.Generated);
        draft.VersionNumber.Should().Be(1);
        draft.Citations.Should().ContainSingle();
    }

    [Fact]
    public async Task GenerateDraft_Twice_ShouldIncrementVersionNumber()
    {
        var client = factory.CreateClient();
        await SeedKnowledgeAsAdminAsync(client);
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");
        var ticket = await CreateTicketAsync(client);

        await client.PostAsJsonAsync($"/api/tickets/{ticket.Id}/drafts/generate", new GenerateDraftRequest(null));
        var response = await client.PostAsJsonAsync($"/api/tickets/{ticket.Id}/drafts/generate", new GenerateDraftRequest("Use a concise tone."));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var draft = await response.Content.ReadFromJsonAsync<DraftDto>();
        draft!.VersionNumber.Should().Be(2);
    }

    [Fact]
    public async Task SendDraft_WithCitations_ShouldMarkDraftSent()
    {
        var client = factory.CreateClient();
        await SeedKnowledgeAsAdminAsync(client);
        await AuthenticateAsync(client, "agent@assistiq.local", "Agent123!");
        var ticket = await CreateTicketAsync(client);
        var draftResponse = await client.PostAsJsonAsync($"/api/tickets/{ticket.Id}/drafts/generate", new GenerateDraftRequest(null));
        var draft = await draftResponse.Content.ReadFromJsonAsync<DraftDto>();

        var response = await client.PostAsync($"/api/drafts/{draft!.Id}/send", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var sentDraft = await response.Content.ReadFromJsonAsync<DraftDto>();
        sentDraft!.Status.Should().Be(DraftStatus.Sent);
    }

    private static async Task<TicketDto> CreateTicketAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest(
            "How do I update billing details?",
            "Linh",
            "linh@example.com"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TicketDto>())!;
    }

    private static async Task SeedKnowledgeAsAdminAsync(HttpClient client)
    {
        await AuthenticateAsync(client, "admin@assistiq.local", "Admin123!");
        var response = await client.PostAsJsonAsync("/api/knowledge-documents", new RegisterKnowledgeDocumentRequest(
            "billing.md",
            "text/markdown",
            512,
            "Billing details can be updated from workspace settings."));
        response.EnsureSuccessStatusCode();
    }

    private static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", login!.Token);
    }
}
