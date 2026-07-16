using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AssistIQ.Api.Controllers;
using AssistIQ.Application.Abstractions;
using AssistIQ.Application.Auth;
using AssistIQ.Application.Common;
using AssistIQ.Application.Drafts;
using AssistIQ.Application.Knowledge;
using AssistIQ.Application.Tickets;
using AssistIQ.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AssistIQ.Tests.Api;

public sealed class RequestInputValidationTests
{
    public static TheoryData<Type, string> JsonBodyActions => new()
    {
        { typeof(AuthController), nameof(AuthController.Login) },
        { typeof(TicketsController), nameof(TicketsController.Create) },
        { typeof(DraftsController), nameof(DraftsController.Generate) },
        { typeof(DraftsController), nameof(DraftsController.Update) },
        { typeof(KnowledgeDocumentsController), nameof(KnowledgeDocumentsController.Register) }
    };

    [Theory]
    [MemberData(nameof(JsonBodyActions))]
    public void BodyAction_ShouldDeclareApplicationJson(Type controllerType, string actionName)
    {
        var action = controllerType.GetMethod(actionName)!;
        var consumes = action.GetCustomAttribute<ConsumesAttribute>();

        consumes.Should().NotBeNull();
        consumes!.ContentTypes.Should().ContainSingle().Which.Should().Be("application/json");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnSafeValidationContract()
    {
        const string secretMarker = "private-password-marker";
        await using var factory = new RequestInputWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("not-an-email", secretMarker));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var rawBody = await response.Content.ReadAsStringAsync();
        rawBody.Should().NotContain(secretMarker).And.NotContain("not-an-email");

        using var body = JsonDocument.Parse(rawBody);
        body.RootElement.EnumerateObject().Select(property => property.Name)
            .Should().BeEquivalentTo("errorCode", "message", "correlationId");
        body.RootElement.GetProperty("errorCode").GetString().Should().Be(ErrorCodes.ValidationFailed);
        body.RootElement.GetProperty("message").GetString().Should().Be("One or more request fields are invalid.");
        body.RootElement.GetProperty("correlationId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithTextPlainBody_ShouldReturnUnsupportedMediaTypeWithoutEchoingCredentials()
    {
        const string credentials = "admin@assistiq.local:private-password-marker";
        await using var factory = new RequestInputWebApplicationFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent(credentials, Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        (await response.Content.ReadAsStringAsync()).Should().NotContain(credentials);
    }

    [Fact]
    public async Task Login_WithApplicationJsonAndCharset_ShouldNotReturnUnsupportedMediaType()
    {
        await using var factory = new RequestInputWebApplicationFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent(
            "{\"email\":\"nobody@example.com\",\"password\":\"wrong-password\"}",
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().NotBe(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Login_WithOversizedBody_ShouldReturnSafePayloadTooLargeContract()
    {
        const string secretMarker = "oversized-private-marker";
        await using var factory = new RequestInputWebApplicationFactory();
        using var client = factory.CreateClient();
        var oversizedPassword = secretMarker + new string('x', 300_000);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@assistiq.local", oversizedPassword));

        response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        var rawBody = await response.Content.ReadAsStringAsync();
        rawBody.Should().NotContain(secretMarker);

        using var body = JsonDocument.Parse(rawBody);
        body.RootElement.GetProperty("errorCode").GetString().Should().Be("request_too_large");
        body.RootElement.GetProperty("correlationId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void LoginRequest_ShouldEnforceBoundaries()
    {
        AssertValid(new LoginRequest(new string('a', 315) + "@x.co", new string('p', 256)));
        AssertInvalid(new LoginRequest(new string('a', 316) + "@x.co", "password"));
        AssertInvalid(new LoginRequest("user@example.com", new string('p', 257)));
        AssertInvalid(new LoginRequest("invalid-email", "password"));
    }

    [Fact]
    public void CreateTicketRequest_ShouldEnforceBoundaries()
    {
        AssertValid(new CreateTicketRequest(
            new string('q', 4_000),
            new string('n', 160),
            new string('a', 315) + "@x.co"));
        AssertInvalid(new CreateTicketRequest(new string('q', 4_001), null, null));
        AssertInvalid(new CreateTicketRequest("question", new string('n', 161), null));
        AssertInvalid(new CreateTicketRequest("question", null, "invalid-email"));
    }

    [Fact]
    public void DraftRequests_ShouldEnforceBoundaries()
    {
        AssertValid(new GenerateDraftRequest(new string('i', 1_000)));
        AssertInvalid(new GenerateDraftRequest(new string('i', 1_001)));
        AssertValid(new UpdateDraftRequest(new string('a', 8_000)));
        AssertInvalid(new UpdateDraftRequest(new string('a', 8_001)));
        AssertInvalid(new UpdateDraftRequest(string.Empty));
    }

    [Fact]
    public void RegisterKnowledgeDocumentRequest_ShouldEnforceBoundaries()
    {
        AssertValid(new RegisterKnowledgeDocumentRequest(
            new string('f', 257) + ".md",
            new string('c', 120),
            5 * 1024 * 1024,
            new string('t', 20_000)));
        AssertInvalid(new RegisterKnowledgeDocumentRequest(new string('f', 261), "text/plain", 1, "text"));
        AssertInvalid(new RegisterKnowledgeDocumentRequest("file.md", new string('c', 121), 1, "text"));
        AssertInvalid(new RegisterKnowledgeDocumentRequest("file.md", "text/plain", 0, "text"));
        AssertInvalid(new RegisterKnowledgeDocumentRequest("file.md", "text/plain", 5 * 1024 * 1024 + 1L, "text"));
        AssertInvalid(new RegisterKnowledgeDocumentRequest("file.md", "text/plain", 1, new string('t', 20_001)));
    }

    private static void AssertValid(object model)
    {
        Validate(model).Should().BeEmpty();
    }

    private static void AssertInvalid(object model)
    {
        Validate(model).Should().NotBeEmpty();
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var modelType = model.GetType();
        var constructor = modelType.GetConstructors().Single();

        foreach (var parameter in constructor.GetParameters())
        {
            var property = modelType.GetProperty(
                parameter.Name!,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)!;
            var context = new ValidationContext(model)
            {
                MemberName = property.Name
            };

            Validator.TryValidateValue(
                property.GetValue(model),
                context,
                results,
                parameter.GetCustomAttributes<ValidationAttribute>());
        }

        return results;
    }

    private sealed class RequestInputWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApplyMigrationsOnStartup"] = "false",
                    ["SeedDemoDataOnStartup"] = "false",
                    ["RateLimiting:LoginPermitLimit"] = "1000",
                    ["RateLimiting:AiDraftPermitLimit"] = "1000"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IUserRepository>();
                services.AddSingleton<IUserRepository, EmptyUserRepository>();
            });
        }
    }

    private sealed class EmptyUserRepository : IUserRepository
    {
        public Task<AppUser?> FindActiveByEmailAsync(string email, CancellationToken cancellationToken)
            => Task.FromResult<AppUser?>(null);

        public Task<AppUser?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<AppUser?>(null);
    }
}
