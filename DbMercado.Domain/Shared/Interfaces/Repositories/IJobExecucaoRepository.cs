using DbMercado.Domain.Shared.Entities;

namespace DbMercado.Domain.Shared.Interfaces.Repositories;

/// <summary>
/// Persistência de histórico de execuções de jobs agendados (Quartz).
/// </summary>
public interface IJobExecucaoRepository
{
    Task<(IReadOnlyList<JobExecucaoGridConsultaLinha> Rows, int RowCount)> ConsultarGridAsync(
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        string? jobNomeContem,
        string? resultadoFiltro,
        int skip,
        int take,
        string sortColumn,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<JobExecucaoEntity?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task RegistrarInicioAsync(
        string fireInstanceId,
        string jobNome,
        string jobGrupo,
        string triggerNome,
        string triggerGrupo,
        DateTimeOffset inicioUtc,
        CancellationToken cancellationToken = default);

    Task FinalizarAsync(
        string fireInstanceId,
        DateTimeOffset fimUtc,
        long duracaoMs,
        bool sucesso,
        string? mensagemErro,
        CancellationToken cancellationToken = default);
}
