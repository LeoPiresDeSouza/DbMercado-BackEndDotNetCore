using DbMercado.Domain.Importacao.Entities;

namespace DbMercado.Domain.Importacao.Interfaces.Repositories;

public interface IProdutoImportadoRepository : IBaseRepository<ProdutoImportadoEntity>
{
    Task<ProdutoImportadoEntity?> GetByCodigoInternoAsync(string codigoInterno, CancellationToken cancellationToken = default);

    Task<ProdutoImportadoEntity?> GetByIdComOrigemAsync(long id, CancellationToken cancellationToken = default);
}
