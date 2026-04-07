using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Chat.Entities;

public class ChatInviteEntity : BaseEntity
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string InvitedBy { get; set; } = string.Empty;

    public string InvitedUserId { get; set; } = string.Empty;

    public ChatInviteStatus Status { get; set; }

    public DateTime ExpiresAt { get; set; }

    public ChatRoomEntity? Room { get; set; }
}
