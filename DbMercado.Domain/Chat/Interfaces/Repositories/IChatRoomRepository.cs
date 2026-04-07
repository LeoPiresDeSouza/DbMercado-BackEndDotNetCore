using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;

namespace DbMercado.Domain.Chat.Interfaces.Repositories;

public interface IChatRoomRepository
{
    Task<ChatRoomEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ChatRoomEntity?> ObterPorIdComMembrosAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatRoomListagemPorUsuario>> ListarSalasDoUsuarioAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(string usuarioAutenticado, ChatRoomEntity entity, CancellationToken cancellationToken = default);

    Task AtualizarAsync(string usuarioAutenticado, ChatRoomEntity entity, CancellationToken cancellationToken = default);
}
