using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Produto.Repositories;

public class ProdutoRepository : BaseRepository<ProdutoEntity>, IProdutoRepository
{
    /// <summary>Cache alinhado ao <see cref="RepositoryFactory"/> (mesmo padrão de ProdutoImportadoRepository).</summary>
    public ProdutoRepository(
        AppDbContext context,
        IApplicationCachingService<ProdutoEntity> _) : base(context)
    {
    }

    public async Task<ProdutoEntity?> GetByIdCompletoAsync(long id, bool rastrear = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!rastrear)
            query = query.AsNoTracking();

        query = query
            .Include(p => p.Skus.OrderBy(s => s.Id));

        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<ProdutoEntity>> ListarCatalogoAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorNcmAsync(string ncm, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.DadosFiscais.Ncm == ncm)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorOrigemGeograficaAsync(
        string tipoOrigemCodigo,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.OrigemProduto.Tipo == tipoOrigemCodigo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorUnidadeMedidaAsync(string unidadeMedida, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.UnidadeMedida.ToUpper() == unidadeMedida.ToUpper())
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorMarcaContendoAsync(string marca, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.Marca != null && p.Marca.ToLower().Contains(marca))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }
}
