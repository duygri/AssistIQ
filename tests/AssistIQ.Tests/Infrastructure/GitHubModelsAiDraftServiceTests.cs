using System.Net;
using System.Text.Json;
using AssistIQ.Application.Abstractions;
using AssistIQ.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AssistIQ.Tests.Infrastructure;

public sealed class GitHubModelsAiDraftServiceTests
{
    [Fact]
    public async Task GenerateAsync_ShouldSendGroundedPromptAndMapResponse()
    {
        var source = new RetrievedSource(
            Guid.NewGuid(),
            "billing.md",
            "file_billing",
            "Billing details can be updated from workspace settings.",
            "result_1",
            0.94m);
        var generatedContent = JsonSerializer.Serialize(new
        {
            answer = "You can update billing details from workspace settings.",
            citations = new[]
            {
                new
                {
                    sourceId = source.KnowledgeDocumentId,
                    quote = "Billing details can be updated from workspace settings."
                }
            }
        });
        var responseJson = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-demo-123",
            choices = new[] { new { message = new { role = "assistant", content = generatedContent } } },
            usage = new { prompt_tokens = 128, completion_tokens = 24 }
        });
        var handler = new RecordingHandler(responseJson);
        var service = CreateService(handler);

        var result = await service.GenerateAsync(
            new TicketDraftInput(
                Guid.NewGuid(),
                "How can I update billing details?",
                "Use a friendly tone.",
                [source]),
            CancellationToken.None);

        result.Answer.Should().Be("You can update billing details from workspace settings.");
        result.Provider.Should().Be("github-models");
        result.Model.Should().Be("openai/gpt-4.1");
        result.ResponseId.Should().Be("chatcmpl-demo-123");
        result.InputTokens.Should().Be(128);
        result.OutputTokens.Should().Be(24);
        result.Citations.Should().ContainSingle()
            .Which.KnowledgeDocumentId.Should().Be(source.KnowledgeDocumentId);

        handler.Request.Should().NotBeNull();
        handler.Request!.RequestUri.Should().Be("https://models.github.ai/inference/chat/completions");
        handler.Request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.Request.Headers.Authorization.Parameter.Should().Be("github-test-token");
        handler.Request.Headers.GetValues("X-GitHub-Api-Version").Should().ContainSingle("2026-03-10");

        using var body = JsonDocument.Parse(handler.RequestBody!);
        body.RootElement.GetProperty("model").GetString().Should().Be("openai/gpt-4.1");
        body.RootElement.GetProperty("response_format").GetProperty("type").GetString().Should().Be("json_object");
        var messages = body.RootElement.GetProperty("messages");
        messages[1].GetProperty("content").GetString().Should().Contain("How can I update billing details?");
        messages[1].GetProperty("content").GetString().Should().Contain("billing.md");
        messages[1].GetProperty("content").GetString().Should().Contain("Use a friendly tone.");
        messages[1].GetProperty("content").GetString().Should().Contain(source.KnowledgeDocumentId.ToString());
    }

    [Fact]
    public async Task GenerateAsync_WithInventedCitation_ShouldRejectResponse()
    {
        var source = new RetrievedSource(
            Guid.NewGuid(),
            "billing.md",
            "file_billing",
            "Billing details can be updated from workspace settings.",
            "result_1",
            0.94m);
        var generatedContent = JsonSerializer.Serialize(new
        {
            answer = "Invented answer",
            citations = new[]
            {
                new { sourceId = Guid.NewGuid(), quote = "Invented quote" }
            }
        });
        var responseJson = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-invalid",
            choices = new[] { new { message = new { content = generatedContent } } }
        });
        var service = CreateService(new RecordingHandler(responseJson));

        var act = () => service.GenerateAsync(
            new TicketDraftInput(Guid.NewGuid(), "Question", null, [source]),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*citation*");
    }

    [Fact]
    public async Task GenerateAsync_WithCaseAlteredQuote_ShouldRejectResponse()
    {
        const string storedText = "Billing Details can be updated from workspace settings.";
        var source = new RetrievedSource(
            Guid.NewGuid(), "billing.md", "file_billing", storedText, "result_1", 0.94m);
        var generatedContent = JsonSerializer.Serialize(new
        {
            answer = "Answer",
            citations = new[]
            {
                new { sourceId = source.KnowledgeDocumentId, quote = storedText.ToLowerInvariant() }
            }
        });
        var responseJson = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-invalid-case",
            choices = new[] { new { message = new { content = generatedContent } } }
        });
        var service = CreateService(new RecordingHandler(responseJson));

        var act = () => service.GenerateAsync(
            new TicketDraftInput(Guid.NewGuid(), "Question", null, [source]),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*citation*");
    }

    [Fact]
    public async Task GenerateAsync_WithOversizedQuote_ShouldRejectResponse()
    {
        var storedText = new string('x', 2_001);
        var source = new RetrievedSource(
            Guid.NewGuid(), "large.md", "file_large", storedText, "result_1", 0.94m);
        var generatedContent = JsonSerializer.Serialize(new
        {
            answer = "Answer",
            citations = new[]
            {
                new { sourceId = source.KnowledgeDocumentId, quote = storedText }
            }
        });
        var responseJson = JsonSerializer.Serialize(new
        {
            id = "chatcmpl-oversized",
            choices = new[] { new { message = new { content = generatedContent } } }
        });
        var service = CreateService(new RecordingHandler(responseJson));

        var act = () => service.GenerateAsync(
            new TicketDraftInput(Guid.NewGuid(), "Question", null, [source]),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*citation*");
    }

    [Fact]
    public async Task GenerateAsync_WithoutToken_ShouldFailBeforeSendingRequest()
    {
        var handler = new RecordingHandler("{}");
        var service = CreateService(handler, token: " ");
        var input = new TicketDraftInput(Guid.NewGuid(), "Question", null, []);

        var act = () => service.GenerateAsync(input, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*GitHub Models token*");
        handler.CallCount.Should().Be(0);
    }

    private static GitHubModelsAiDraftService CreateService(
        HttpMessageHandler handler,
        string token = "github-test-token")
    {
        var options = Options.Create(new GitHubModelsOptions
        {
            Model = "openai/gpt-4.1",
            Token = token
        });

        return new GitHubModelsAiDraftService(new HttpClient(handler), options);
    }

    private sealed class RecordingHandler(string responseJson) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };
        }
    }
}
