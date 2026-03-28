using DbMercado.Domain.Importacao.Entities;
using DbMercado.Domain.Importacao.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Importacao.Repositories;

public class ProdutoImportadoRepository : BaseRepository<ProdutoImportadoEntity>, IProdutoImportadoRepository
{
    public ProdutoImportadoRepository(
        AppDbContext context,
        IApplicationCachingService<ProdutoImportadoEntity> _) : base(context)
    {
    }

    public async Task<ProdutoImportadoEntity?> GetByCodigoInternoAsync(string codigoInterno, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CodigoInterno == codigoInterno, cancellationToken);
    }

    public async Task<ProdutoImportadoEntity?> GetByIdComOrigemAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(p => p.ItemNotaFiscalOrigem!)
                .ThenInclude(i => i.NotaFiscal)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
