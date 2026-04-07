using DbMercado.Domain.Chat;

namespace DbMercado.Application.Chat.Dtos;

/// <summary>
/// Payload enviado ao cliente via SignalR ao receber uma mensagem na sala.
/// <see cref="SenderId"/> alinhado ao ASP.NET Identity (<c>IdentityUser.Id</c>).
/// </summary>
public sealed class MessageDto
{
    public Guid MessageId { get; init; }

    /// <summary>Sala da mensagem (para o cliente rotear em cenários com múltiplos grupos).</summary>
    public Guid RoomId { get; init; }

    public string SenderId { get; init; } = string.Empty;

    public string SenderName { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public bool IsTranslated { get; init; }

    /// <summary>Estado da tradução no servidor (serializado em camelCase no JSON do hub).</summary>
    public MessageTranslationStatus TranslationStatus { get; init; }

    /// <summary>Código BCP-47 do idioma de origem (ex.: pt-BR).</summary>
    public string SourceLang { get; init; } = string.Empty;

    /// <summary>Instante de envio em UTC.</summary>
    public DateTime SentAt { get; init; }
}
