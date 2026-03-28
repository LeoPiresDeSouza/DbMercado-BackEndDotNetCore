using DbMercado.Domain.Importacao.Interfaces.Repositories;

namespace DbMercado.Domain.Importacao.Interfaces.UnitsOfWork;

public interface IUwImportacao
{
    INotaFiscalRepository NotaFiscalRepository { get; }

    IProdutoImportadoRepository ProdutoImportadoRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
