namespace DbMercado.Application.Chat.Dtos;

/// <summary>
/// Página de mensagens de uma sala (<c>GET /api/chat/rooms/{id}/messages</c>), com textos já no idioma do membro solicitante.
/// </summary>
public sealed class ChatMessagesPageResponse
{
    public IReadOnlyList<MessageDto> Items { get; init; } = Array.Empty<MessageDto>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}
