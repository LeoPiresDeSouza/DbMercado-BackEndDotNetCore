using System.Globalization;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.Repositories;
using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Chat.Repositories;

public sealed class ChatInviteRepository : IChatInviteRepository
{
    private readonly AppDbContext _context;

    /// <summary>Cache alinhado ao <see cref="DbMercado.Infrastructure.Shared.Repositories.RepositoryFactory"/> e invalidação na <see cref="DbMercado.Infrastructure.Chat.UnitsOfWork.UwChat"/>.</summary>
    public ChatInviteRepository(AppDbContext context, IApplicationCachingService<ChatInviteEntity> _)
    {
        _context = context;
    }

    public Task<bool> ExisteConvitePendenteAsync(
        Guid roomId,
        string invitedUserId,
        CancellationToken cancellationToken = default) =>
        _context.ChatInvites.AsNoTracking().AnyAsync(
            i => i.RoomId == roomId
                 && i.InvitedUserId == invitedUserId
                 && i.Status == ChatInviteStatus.Pending,
            cancellationToken);

    public async Task<IReadOnlyList<ChatInviteEntity>> ListarPendentesNaoExpiradosParaConvidadoAsync(
        string invitedUserId,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTime.UtcNow;
        var lista = await _context.ChatInvites
            .AsNoTracking()
            .Include(i => i.Room)
            .Where(i =>
                i.InvitedUserId == invitedUserId
                && i.Status == ChatInviteStatus.Pending
                && i.ExpiresAt > agora
                && i.Room != null
                && i.Room.Status == ChatRoomStatus.Active)
            .OrderBy(i => i.ExpiresAt)
            .ToListAsync(cancellationToken);

        return lista;
    }

    public Task<ChatInviteEntity?> ObterPorIdComTrackingAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ChatInvites.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task AdicionarAsync(
        string usuarioAutenticado,
        ChatInviteEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaInclusao(entity, usuarioAutenticado);
        await _context.ChatInvites.AddAsync(entity, cancellationToken);
    }

    public Task AtualizarAsync(
        string usuarioAutenticado,
        ChatInviteEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaAlteracao(entity, usuarioAutenticado);
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.ChatInvites.Attach(entity);
            entry.State = EntityState.Modified;
        }

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
