using System.Globalization;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.Repositories;
using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Chat.Repositories;

public sealed class ChatRoomRepository : IChatRoomRepository
{
    private readonly AppDbContext _context;

    /// <summary>Cache alinhado ao <see cref="DbMercado.Infrastructure.Shared.Repositories.RepositoryFactory"/> e invalidação na <see cref="DbMercado.Infrastructure.Chat.UnitsOfWork.UwChat"/>.</summary>
    public ChatRoomRepository(AppDbContext context, IApplicationCachingService<ChatRoomEntity> _)
    {
        _context = context;
    }

    public Task<ChatRoomEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ChatRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<ChatRoomEntity?> ObterPorIdComMembrosAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ChatRooms
            .AsNoTracking()
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ChatRoomListagemPorUsuario>> ListarSalasDoUsuarioAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var linhas = await _context.ChatRooms
            .AsNoTracking()
            .Where(r => r.Members.Any(m => m.UserId == userId))
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.Status,
                MemberCount = r.Members.Count,
                MyRole = r.Members.Where(m => m.UserId == userId).Select(m => m.Role).First(),
                MyLanguagePref = r.Members.Where(m => m.UserId == userId).Select(m => m.LanguagePref).First(),
                LastMessageAt = _context.Messages
                    .Where(m => m.RoomId == r.Id && m.DeletedAt == null)
                    .Select(m => (DateTime?)m.SentAt)
                    .Max()
            })
            .ToListAsync(cancellationToken);

        return linhas
            .OrderByDescending(x => x.LastMessageAt ?? DateTime.MinValue)
            .Select(x => new ChatRoomListagemPorUsuario(
                x.Id,
                x.Name,
                x.Description,
                x.Status,
                x.MemberCount,
                x.MyRole,
                x.MyLanguagePref,
                x.LastMessageAt))
            .ToList();
    }

    public async Task AdicionarAsync(
        string usuarioAutenticado,
        ChatRoomEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaInclusao(entity, usuarioAutenticado);
        await _context.ChatRooms.AddAsync(entity, cancellationToken);
    }

    public Task AtualizarAsync(
        string usuarioAutenticado,
        ChatRoomEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaAlteracao(entity, usuarioAutenticado);
        _context.ChatRooms.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    private static void PreencherAuditoriaInclusao(BaseEntity entity, string usuarioAutenticado)
    {
        var data = DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("pt-BR"));
        entity.DataCriacao = data;
        entity.DataUltimaAlteracao = data;
        entity.UsuarioCriacao = usuarioAutenticado;
        entity.UsuarioUltimaAlteracao = usuarioAutenticado;
    }

    private static void PreencherAuditoriaAlteracao(BaseEntity entity, string usuarioAutenticado)
    {
        var data = DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("pt-BR"));
        entity.DataUltimaAlteracao = data;
        entity.UsuarioUltimaAlteracao = usuarioAutenticado;
    }
}
