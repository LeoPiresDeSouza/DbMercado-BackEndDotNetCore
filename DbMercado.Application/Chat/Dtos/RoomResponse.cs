namespace DbMercado.Application.Chat.Dtos;

public sealed class RoomResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    /// <summary>Valor do enum <see cref="DbMercado.Domain.Chat.ChatRoomStatus"/> serializado.</summary>
    public string Status { get; init; } = string.Empty;

    public int MemberCount { get; init; }

    /// <summary>Valor do enum <see cref="DbMercado.Domain.Chat.ChatMemberRole"/> para o usuário solicitante.</summary>
    public string MyRole { get; init; } = string.Empty;

    public string MyLanguagePref { get; init; } = string.Empty;

    /// <summary>UTC da última mensagem ativa na sala, se houver.</summary>
    public DateTime? LastMessageAt { get; init; }
}
