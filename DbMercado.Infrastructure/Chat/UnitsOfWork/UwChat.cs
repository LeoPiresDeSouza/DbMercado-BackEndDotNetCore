using DbMercado.Domain.Chat.Interfaces.Repositories;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Chat.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.UnitsOfWork;

namespace DbMercado.Infrastructure.Chat.UnitsOfWork;

public sealed class UwChat : IUwChat
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;
    private readonly IApplicationCachingFactory _cacheFactory;

    private IChatRoomRepository? _salas;
    private IChatMemberRepository? _membros;
    private IChatInviteRepository? _convites;
    private IMessageRepository? _mensagens;

    public UwChat(
        AppDbContext context,
        IRepositoryFactory repoFactory,
        IApplicationCachingFactory cacheFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
        _cacheFactory = cacheFactory;
    }

    public IChatRoomRepository Salas =>
        _salas ??= _repoFactory.Create<ChatRoomRepository>(_context);

    public IChatMemberRepository Membros =>
        _membros ??= _repoFactory.Create<ChatMemberRepository>(_context);

    public IChatInviteRepository Convites =>
        _convites ??= _repoFactory.Create<ChatInviteRepository>(_context);

    public IMessageRepository Mensagens =>
        _mensagens ??= _repoFactory.Create<MessageRepository>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var linhas = await _context.SaveChangesAsync(cancellationToken);
        UnitOfWorkCacheInvalidacao.AposSaveSeAlterou(
            _cacheFactory,
            linhas,
            UnitOfWorkCacheInvalidacao.Chat);
        return linhas;
    }
}
