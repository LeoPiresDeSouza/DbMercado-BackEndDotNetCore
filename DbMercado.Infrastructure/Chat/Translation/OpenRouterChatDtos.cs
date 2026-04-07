using System.Text.Json.Serialization;

namespace DbMercado.Infrastructure.Chat.Translation;

internal sealed class OpenRouterChatCompletionRequest
{
    public required string Model { get; init; }
    public required double Temperature { get; init; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; init; }

    public required IReadOnlyList<OpenRouterChatMessageDto> Messages { get; init; }
}

internal sealed class OpenRouterChatMessageDto
{
    public required string Role { get; init; }
    public required string Content { get; init; }
}

internal sealed class OpenRouterChatCompletionResponse
{
    public List<OpenRouterChatChoiceDto>? Choices { get; init; }
}

internal sealed class OpenRouterChatChoiceDto
{
    public OpenRouterChatMessageDto? Message { get; init; }
}
