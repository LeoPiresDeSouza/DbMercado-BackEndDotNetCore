namespace DbMercado.Application.Chat.Dtos;

public sealed class InviteResponse
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public string InvitedByName { get; set; } = string.Empty;

    /// <summary>UTC — expiração do convite (48h após a criação).</summary>
    public DateTime ExpiresAt { get; set; }
}
