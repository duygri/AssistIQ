using System.Net;
using System.Net.Http.Json;
using AssistIQ.Application.Auth;
using FluentAssertions;

namespace AssistIQ.Tests.Api;

public sealed class AuthApiTests(CustomWebApplicationFactory factory)
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
    public async Task Login_WithSeededAdmin_ShouldReturnTokenAndAdminRole()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@assistiq.local",
            "Admin123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithSeededSupportAgent_ShouldReturnTokenAndAgentRole()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "agent@assistiq.local",
            "Agent123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.User.Role.Should().Be("SupportAgent");
    }

    [Fact]
    public async Task Login_WithBadPassword_ShouldReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "admin@assistiq.local",
            "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_ShouldReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithSupportAgentToken_ShouldReturnCurrentUser()
    {
        var client = factory.CreateClient();
        var login = await LoginAsync(client, "agent@assistiq.local", "Agent123!");

        client.DefaultRequestHeaders.Authorization = new("Bearer", login.Token);
        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("agent@assistiq.local");
        body.Role.Should().Be("SupportAgent");
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }
}
