using DbMercado.Domain.Produto.Entities;

namespace DbMercado.Domain.Produto.Interfaces.Repositories;

public interface ICategoriaProdutoRepository
{
    Task<IReadOnlyList<CategoriaProdutoEntity>> ListarArvoreCompletaAsync(CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaProdutoEntity>> ListarPorNivelAsync(int nivel, CancellationToken ct = default);

    Task<CategoriaProdutoEntity?> ObterPorIdAsync(long id, CancellationToken ct = default);

    Task<CategoriaProdutoEntity?> ObterPorSlugAsync(string slug, CancellationToken ct = default);

    Task<bool> ExisteFilhaAsync(long categoriaId, CancellationToken ct = default);

    Task<bool> ExisteProdutoVinculadoAsync(long categoriaId, CancellationToken ct = default);

    void Adicionar(CategoriaProdutoEntity categoria);

    void Remover(CategoriaProdutoEntity categoria);
}
