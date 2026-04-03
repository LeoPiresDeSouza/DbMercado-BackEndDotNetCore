using DbMercado.Domain.Produto.Interfaces.Repositories;

namespace DbMercado.Domain.Produto.Interfaces.UnitsOfWork;

public interface IUwProduto
{
    IProdutoRepository ProdutoRepository { get; }

    ICategoriaProdutoRepository Categorias { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
