namespace DbMercado.Domain.Chat;

/// <summary>Linha de listagem de salas para um usuário membro (consulta / API).</summary>
public sealed record ChatRoomListagemPorUsuario(
    Guid Id,
    string Name,
    string? Description,
    ChatRoomStatus Status,
    int MemberCount,
    ChatMemberRole MyRole,
    string MyLanguagePref,
    DateTime? LastMessageAt);
