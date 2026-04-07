using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Chat.Entities;

public class ChatMemberEntity : BaseEntity
{
    public Guid RoomId { get; set; }

    public string UserId { get; set; } = string.Empty;

    /// <summary>Código BCP-47; validar contra <see cref="DbMercado.Domain.Chat.LanguageCode.All"/> na aplicação.</summary>
    public string LanguagePref { get; set; } = string.Empty;

    public ChatMemberRole Role { get; set; }

    /// <summary>UTC — instante em que o membro entrou na sala.</summary>
    public DateTime JoinedAt { get; set; }

    public ChatRoomEntity? Room { get; set; }
}
