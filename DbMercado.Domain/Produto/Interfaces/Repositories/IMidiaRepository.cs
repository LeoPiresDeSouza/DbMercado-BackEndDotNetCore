using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Shared.Interfaces.Repositories;

namespace DbMercado.Domain.Produto.Interfaces.Repositories;

public interface IMidiaRepository : IBaseRepository<MidiaEntity>
{
    Task<IReadOnlyList<MidiaEntity>> ListarPorProdutoIdOrdenadasAsync(
        long produtoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MidiaEntity>> ListarTemporariasExpiradasAsync(
        DateTime criadasAntesDeUtc,
        CancellationToken cancellationToken = default);
}
