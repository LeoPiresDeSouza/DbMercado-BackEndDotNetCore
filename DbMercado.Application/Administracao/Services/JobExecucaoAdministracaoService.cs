using DbMercado.Application.Administracao.Dtos.JobExecucao;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Domain.Shared.Interfaces.Repositories;

namespace DbMercado.Application.Administracao.Services;

public sealed class JobExecucaoAdministracaoService : IJobExecucaoAdministracaoService
{
    private readonly IJobExecucaoRepository _repository;

    public JobExecucaoAdministracaoService(IJobExecucaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<JobExecucaoGridResultDto> ConsultarGridAsync(
        JobExecucaoGridQueryDto query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var take = Math.Clamp(query.EndRow - query.StartRow, 0, 500);
        var skip = Math.Max(0, query.StartRow);
        var (col, desc) = ResolverOrdenacao(query.SortModel);

        var (rows, total) = await _repository.ConsultarGridAsync(
            query.DataInicio,
            query.DataFim,
            string.IsNullOrWhiteSpace(query.JobNome) ? null : query.JobNome.Trim(),
            string.IsNullOrWhiteSpace(query.ResultadoFiltro) ? null : query.ResultadoFiltro.Trim(),
            skip,
            take,
            col,
            desc,
            cancellationToken);

        return new JobExecucaoGridResultDto
        {
            RowCount = total,
            Rows = rows.Select(e => new JobExecucaoGridRowDto
            {
                Id = e.Id,
                FireInstanceId = e.FireInstanceId,
                JobNome = e.JobNome,
                JobGrupo = e.JobGrupo,
                TriggerNome = e.TriggerNome,
                TriggerGrupo = e.TriggerGrupo,
                InicioUtc = e.InicioUtc,
                FimUtc = e.FimUtc,
                DuracaoMs = e.DuracaoMs,
                Sucesso = e.Sucesso,
                MensagemErro = e.MensagemErro
            }).ToList()
        };
    }

    public async Task<JobExecucaoDetalheDto?> ObterDetalheAsync(long id, CancellationToken cancellationToken = default)
    {
        var e = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (e is null)
            return null;

        return new JobExecucaoDetalheDto
        {
            Id = e.Id,
            FireInstanceId = e.FireInstanceId,
            JobNome = e.JobNome,
            JobGrupo = e.JobGrupo,
            TriggerNome = e.TriggerNome,
            TriggerGrupo = e.TriggerGrupo,
            InicioUtc = e.InicioUtc,
            FimUtc = e.FimUtc,
            DuracaoMs = e.DuracaoMs,
            Sucesso = e.Sucesso,
            MensagemErro = e.MensagemErro
        };
    }

    private static (string Column, bool Descending) ResolverOrdenacao(List<JobExecucaoGridSortItemDto>? sortModel)
    {
        if (sortModel is not { Count: > 0 })
            return ("inicioUtc", true);

        var first = sortModel[0];
        var col = (first.ColId ?? string.Empty).Trim().ToLowerInvariant();
        var desc = string.Equals(first.Sort, "desc", StringComparison.OrdinalIgnoreCase);

        return col switch
        {
            "fireinstanceid" => ("fireInstanceId", desc),
            "jobnome" => ("jobNome", desc),
            "jobgrupo" => ("jobGrupo", desc),
            "triggernome" => ("triggerNome", desc),
            "triggergrupo" => ("triggerGrupo", desc),
            "inicioutc" => ("inicioUtc", desc),
            "fimutc" => ("fimUtc", desc),
            "duracaoms" => ("duracaoMs", desc),
            "sucesso" => ("sucesso", desc),
            "mensagemerro" => ("mensagemErro", desc),
            _ => ("inicioUtc", desc)
        };
    }
}
