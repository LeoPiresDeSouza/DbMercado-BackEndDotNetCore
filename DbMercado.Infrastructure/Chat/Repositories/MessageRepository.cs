using System.Globalization;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.Repositories;
using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Chat.Repositories;

public sealed class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    /// <summary>Cache alinhado ao <see cref="DbMercado.Infrastructure.Shared.Repositories.RepositoryFactory"/> e invalidação na <see cref="DbMercado.Infrastructure.Chat.UnitsOfWork.UwChat"/>.</summary>
    public MessageRepository(AppDbContext context, IApplicationCachingService<MessageEntity> _)
    {
        _context = context;
    }

    public Task<MessageEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<MessageEntity?> ObterPorIdComTraducoesAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Messages
            .AsNoTracking()
            .Include(m => m.Translations)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<MessageEntity> Itens, int Total)> ListarPorSalaPaginadoAsync(
        Guid roomId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Messages
            .AsNoTracking()
            .Where(m => m.RoomId == roomId && m.DeletedAt == null);

        var total = await baseQuery.CountAsync(cancellationToken);

        var itens = await baseQuery
            .Include(m => m.Translations)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task AdicionarAsync(
        string usuarioAutenticado,
        MessageEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaInclusao(entity, usuarioAutenticado);
        await _context.Messages.AddAsync(entity, cancellationToken);
    }

    public Task AtualizarAsync(
        string usuarioAutenticado,
        MessageEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaAlteracao(entity, usuarioAutenticado);
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.Messages.Attach(entity);
            entry.State = EntityState.Modified;
        }

        return Task.CompletedTask;
    }

    public async Task RegistrarOuAtualizarLeituraAsync(
        string usuarioAutenticado,
        Guid messageId,
        string userId,
        DateTime readAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existente = await _context.MessageReceipts
            .FirstOrDefaultAsync(
                r => r.MessageId == messageId && r.UserId == userId,
                cancellationToken);

        if (existente is null)
        {
            var novo = new MessageReceiptEntity
            {
                MessageId = messageId,
                UserId = userId,
                ReadAt = readAtUtc,
                DeliveredAt = null
            };
            PreencherAuditoriaInclusao(novo, usuarioAutenticado);
            await _context.MessageReceipts.AddAsync(novo, cancellationToken);
            return;
        }

        existente.ReadAt = readAtUtc;
        PreencherAuditoriaAlteracao(existente, usuarioAutenticado);
    }

    public async Task<IReadOnlyList<MessageEntity>> ListarMensagensTraducaoPendenteAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m =>
                m.TranslationStatus == MessageTranslationStatus.Pending &&
                m.DeletedAt == null)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);
    }

    public Task<MessageEntity?> ObterPorIdComTrackingAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> TraducaoExisteAsync(
        Guid messageId,
        string targetLang,
        CancellationToken cancellationToken = default) =>
        _context.MessageTranslations.AsNoTracking().AnyAsync(
            t => t.MessageId == messageId && t.TargetLang == targetLang,
            cancellationToken);

    public async Task AdicionarTraducaoAsync(
        string usuarioAutenticado,
        MessageTranslationEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        PreencherAuditoriaInclusao(entity, usuarioAutenticado);
        await _context.MessageTranslations.AddAsync(entity, cancellationToken);
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
