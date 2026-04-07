using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Chat.Entities;

public class MessageTranslationEntity : BaseEntity
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string TargetLang { get; set; } = string.Empty;

    public string TranslatedText { get; set; } = string.Empty;

    public bool FromCache { get; set; }

    public DateTime TranslatedAt { get; set; }

    public MessageEntity? Message { get; set; }
}
