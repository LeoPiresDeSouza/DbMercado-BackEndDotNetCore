using DbMercado.Domain.Importacao.Entities;

namespace DbMercado.Domain.Importacao.Interfaces.Repositories;

public interface INotaFiscalRepository : IBaseRepository<NotaFiscalEntity>
{
    Task<NotaFiscalEntity?> GetByIdWithItensAsync(long id, CancellationToken cancellationToken = default);

    Task<NotaFiscalEntity?> GetByChaveAcessoAsync(string chaveAcesso, CancellationToken cancellationToken = default);

    /// <summary>Persiste a nota e os itens, preenchendo auditoria em toda a árvore.</summary>
    Task AddComItensAsync(string authenticatedUserName, NotaFiscalEntity notaFiscal, CancellationToken cancellationToken = default);

    Task<bool> ItemNotaFiscalExistsAsync(long itemNotaFiscalId, CancellationToken cancellationToken = default);
}
