using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Shared.Interfaces.Repositories;

/// <summary>
/// Leitura e exclusão de registros persistidos em <c>dbAppLog</c>.
/// </summary>
public interface IAppLogRepository
{
    Task<(IReadOnlyList<AppLogGridConsultaLinha> Rows, int RowCount)> ConsultarGridAsync(
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        bool somenteComExcecao,
        IReadOnlyList<string>? levels,
        int skip,
        int take,
        string sortColumn,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<AppLogEntity?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExcluirPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<int> ContarTotalAsync(CancellationToken cancellationToken = default);

    /// <summary>Os registros mais antigos primeiro (para backup + exclusão).</summary>
    Task<IReadOnlyList<AppLogEntity>> ObterMaisAntigosAsync(int quantidade, CancellationToken cancellationToken = default);

    Task<int> ExcluirPorIdsAsync(IReadOnlyList<long> ids, CancellationToken cancellationToken = default);
}
