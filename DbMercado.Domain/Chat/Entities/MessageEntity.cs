using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Chat.Entities;

public class MessageEntity : BaseEntity
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string SenderId { get; set; } = string.Empty;

    public string ContentOriginal { get; set; } = string.Empty;

    /// <summary>Código BCP-47 do idioma de origem.</summary>
    public string SourceLang { get; set; } = string.Empty;

    public MessageType MessageType { get; set; }

    public MessageTranslationStatus TranslationStatus { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ChatRoomEntity? Room { get; set; }

    public ICollection<MessageTranslationEntity> Translations { get; set; } = new List<MessageTranslationEntity>();

    public ICollection<MessageReceiptEntity> Receipts { get; set; } = new List<MessageReceiptEntity>();
}
