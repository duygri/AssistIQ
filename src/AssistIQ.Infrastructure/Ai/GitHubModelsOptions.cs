namespace AssistIQ.Infrastructure.Ai;

public sealed class GitHubModelsOptions
{
    public string Model { get; set; } = "openai/gpt-4.1";

    public string Token { get; set; } = string.Empty;
}
