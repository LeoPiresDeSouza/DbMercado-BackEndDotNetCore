using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Chat.Entities;

public class MessageReceiptEntity : BaseEntity
{
    public Guid MessageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public MessageEntity? Message { get; set; }
}
