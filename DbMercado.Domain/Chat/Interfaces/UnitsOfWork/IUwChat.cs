using DbMercado.Domain.Chat.Interfaces.Repositories;

namespace DbMercado.Domain.Chat.Interfaces.UnitsOfWork;

public interface IUwChat
{
    IChatRoomRepository Salas { get; }

    IChatMemberRepository Membros { get; }

    IChatInviteRepository Convites { get; }

    IMessageRepository Mensagens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
