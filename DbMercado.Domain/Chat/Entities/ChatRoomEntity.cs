using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Chat.Entities;

public class ChatRoomEntity : BaseEntity
{
    public Guid Id { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ChatRoomStatus Status { get; set; }

    public ICollection<ChatMemberEntity> Members { get; set; } = new List<ChatMemberEntity>();

    public ICollection<ChatInviteEntity> Invites { get; set; } = new List<ChatInviteEntity>();

    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
}
