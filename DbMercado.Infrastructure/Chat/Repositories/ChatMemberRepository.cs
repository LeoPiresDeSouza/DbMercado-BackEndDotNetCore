using System.Globalization;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.Repositories;
using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Chat.Repositories;

public sealed class ChatMemberRepository : IChatMemberRepository
{
    private readonly AppDbContext _context;

    /// <summary>Cache alinhado ao <see cref="DbMercado.Infrastructure.Shared.Repositories.RepositoryFactory"/> e invalidação na <see cref="DbMercado.Infrastructure.Chat.UnitsOfWork.UwChat"/>.</summary>
    public ChatMemberRepository(AppDbContext context, IApplicationCachingService<ChatMemberEntity> _)
    {
        _context = context;
    }

    public Task<bool> ExisteMembroNaSalaAsync(
        Guid roomId,
        string userId,
        CancellationToken cancellationToken = default) =>
        _context.ChatMembers.AsNoTracking().AnyAsync(
            m => m.RoomId == roomId && m.UserId == userId,
            cancellationToken);

    public Task<string?> ObterLanguagePrefMembroAsync(
        Guid roomId,
        string userId,
        CancellationToken cancellationToken = default) =>
        _context.ChatMembers.AsNoTracking()
            .Where(m => m.RoomId == roomId && m.UserId == userId)
            .Select(m => m.LanguagePref)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ChatMemberEntity?> ObterPorSalaEUsuarioComTrackingAsync(
        Guid roomId,
        string userId,
        CancellationToken cancellationToken = default) =>
        _context.ChatMembers.FirstOrDefaultAsync(
            m => m.RoomId == roomId && m.UserId == userId,
            cancellationToken);

    public Task AtualizarAsync(
        string usuarioAutenticado,
        ChatMemberEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaAlteracao(entity, usuarioAutenticado);
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.ChatMembers.Attach(entity);
            entry.State = EntityState.Modified;
        }

        return Task.CompletedTask;
    }

    public async Task AdicionarAsync(
        string usuarioAutenticado,
        ChatMemberEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaInclusao(entity, usuarioAutenticado);
        await _context.ChatMembers.AddAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<(string UserId, string LanguagePref)>> ListarUsuarioEIdiomaPorSalaAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.ChatMembers
            .AsNoTracking()
            .Where(m => m.RoomId == roomId)
            .Select(m => new { m.UserId, m.LanguagePref })
            .ToListAsync(cancellationToken);

        return rows.Select(r => (r.UserId, r.LanguagePref)).ToList();
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
