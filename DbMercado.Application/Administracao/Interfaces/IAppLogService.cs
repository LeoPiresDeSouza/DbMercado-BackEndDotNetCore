using DbMercado.Application.Administracao.Dtos.AppLog;

namespace DbMercado.Application.Administracao.Interfaces;

public interface IAppLogService
{
    Task<AppLogGridResultDto> ConsultarGridAsync(AppLogGridQueryDto query, CancellationToken cancellationToken = default);

    Task<AppLogDetalheDto?> ObterDetalheAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExcluirEntradaAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Mesma lógica do job Quartz: backup em disco + exclusão por política min/max.</summary>
    Task<LogLimpezaResultDto> ExecutarLimpezaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LogBackupArquivoDto>> ListarBackupsAsync(CancellationToken cancellationToken = default);

    Task<(string Conteudo, string NomeArquivo)?> ObterConteudoBackupAsync(string nomeArquivo, CancellationToken cancellationToken = default);
}
