using DbMercado.Application.Administracao.Dtos.JobExecucao;

namespace DbMercado.Application.Administracao.Interfaces;

public interface IJobExecucaoAdministracaoService
{
    Task<JobExecucaoGridResultDto> ConsultarGridAsync(JobExecucaoGridQueryDto query, CancellationToken cancellationToken = default);

    Task<JobExecucaoDetalheDto?> ObterDetalheAsync(long id, CancellationToken cancellationToken = default);
}
