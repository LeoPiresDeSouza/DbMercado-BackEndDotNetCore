using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Produto.Repositories;

public sealed class MidiaRepository : BaseRepository<MidiaEntity>, IMidiaRepository
{
    /// <summary>Cache injetado pelo <see cref="RepositoryFactory"/> (mesmo contrato que <see cref="ProdutoRepository"/>).</summary>
    public MidiaRepository(AppDbContext context, IApplicationCachingService<MidiaEntity> _) : base(context)
    {
    }

    public async Task<IReadOnlyList<MidiaEntity>> ListarPorProdutoIdOrdenadasAsync(
        long produtoId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(m => m.ProdutoId == produtoId && m.Status == "ativo")
            .OrderBy(m => m.Ordem)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MidiaEntity>> ListarTemporariasExpiradasAsync(
        DateTime criadasAntesDeUtc,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.Status == "temporario" && m.DataCriacao < criadasAntesDeUtc)
            .ToListAsync(cancellationToken);
    }
}
