using System.Globalization;
using DbMercado.Domain.Importacao.Entities;
using DbMercado.Domain.Importacao.Interfaces.Repositories;
using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Importacao.Repositories;

public class NotaFiscalRepository : BaseRepository<NotaFiscalEntity>, INotaFiscalRepository
{
    /// <summary>Cache não utilizado neste repositório; parâmetro exige compatibilidade com <see cref="IRepositoryFactory"/>.</summary>
    public NotaFiscalRepository(
        AppDbContext context,
        IApplicationCachingService<NotaFiscalEntity> _) : base(context)
    {
    }

    public async Task<NotaFiscalEntity?> GetByIdWithItensAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.Itens.OrderBy(i => i.NumeroItem))
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<NotaFiscalEntity?> GetByChaveAcessoAsync(string chaveAcesso, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(n => n.Itens.OrderBy(i => i.NumeroItem))
            .FirstOrDefaultAsync(n => n.ChaveAcesso == chaveAcesso, cancellationToken);
    }

    public async Task AddComItensAsync(string authenticatedUserName, NotaFiscalEntity notaFiscal, CancellationToken cancellationToken = default)
    {
        var data = DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("pt-BR"));
        var usuario = authenticatedUserName;
        ApplyAudit(notaFiscal, data, usuario);
        foreach (var item in notaFiscal.Itens)
            ApplyAudit(item, data, usuario);

        await DbSet.AddAsync(notaFiscal, cancellationToken);
    }

    public Task<bool> ItemNotaFiscalExistsAsync(long itemNotaFiscalId, CancellationToken cancellationToken = default)
    {
        return _context.Set<ItemNotaFiscalEntity>()
            .AsNoTracking()
            .AnyAsync(i => i.Id == itemNotaFiscalId, cancellationToken);
    }

    private static void ApplyAudit(BaseEntity entity, DateTime data, string usuario)
    {
        entity.DataCriacao = data;
        entity.DataUltimaAlteracao = data;
        entity.UsuarioCriacao = usuario;
        entity.UsuarioUltimaAlteracao = usuario;
    }
}
