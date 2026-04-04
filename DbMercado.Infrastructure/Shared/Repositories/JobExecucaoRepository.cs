using DbMercado.Domain.Shared.Entities;
using DbMercado.Domain.Shared.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Shared.Repositories;

public sealed class JobExecucaoRepository : IJobExecucaoRepository
{
    private readonly AppDbContext _context;

    public JobExecucaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<JobExecucaoGridConsultaLinha> Rows, int RowCount)> ConsultarGridAsync(
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        string? jobNomeContem,
        string? resultadoFiltro,
        int skip,
        int take,
        string sortColumn,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = BuildFilteredQuery(dataInicio, dataFim, jobNomeContem, resultadoFiltro);

        var total = await baseQuery.CountAsync(cancellationToken);

        var ordered = ApplyOrdenacao(baseQuery, sortColumn, sortDescending);

        var rows = await ordered
            .Skip(skip)
            .Take(take)
            .Select(e => new JobExecucaoGridConsultaLinha
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
            })
            .ToListAsync(cancellationToken);

        return (rows, total);
    }

    public Task<JobExecucaoEntity?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        _context.Set<JobExecucaoEntity>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task RegistrarInicioAsync(
        string fireInstanceId,
        string jobNome,
        string jobGrupo,
        string triggerNome,
        string triggerGrupo,
        DateTimeOffset inicioUtc,
        CancellationToken cancellationToken = default)
    {
        var row = new JobExecucaoEntity
        {
            FireInstanceId = fireInstanceId,
            JobNome = jobNome,
            JobGrupo = jobGrupo,
            TriggerNome = triggerNome,
            TriggerGrupo = triggerGrupo,
            InicioUtc = inicioUtc,
        };
        _context.Set<JobExecucaoEntity>().Add(row);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task FinalizarAsync(
        string fireInstanceId,
        DateTimeOffset fimUtc,
        long duracaoMs,
        bool sucesso,
        string? mensagemErro,
        CancellationToken cancellationToken = default)
    {
        var row = await _context.Set<JobExecucaoEntity>()
            .FirstOrDefaultAsync(e => e.FireInstanceId == fireInstanceId, cancellationToken);
        if (row is null)
            return;

        row.FimUtc = fimUtc;
        row.DuracaoMs = duracaoMs;
        row.Sucesso = sucesso;
        row.MensagemErro = Truncar(mensagemErro, 4000);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<JobExecucaoEntity> BuildFilteredQuery(
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        string? jobNomeContem,
        string? resultadoFiltro)
    {
        var query = _context.Set<JobExecucaoEntity>().AsNoTracking();

        if (dataInicio.HasValue)
            query = query.Where(e => e.InicioUtc >= dataInicio.Value);
        if (dataFim.HasValue)
            query = query.Where(e => e.InicioUtc <= dataFim.Value);

        if (!string.IsNullOrWhiteSpace(jobNomeContem))
        {
            var term = jobNomeContem.Trim();
            query = query.Where(e => e.JobNome.Contains(term));
        }

        query = AplicarFiltroResultado(query, resultadoFiltro);

        return query;
    }

    private static IQueryable<JobExecucaoEntity> AplicarFiltroResultado(
        IQueryable<JobExecucaoEntity> query,
        string? resultadoFiltro)
    {
        if (string.IsNullOrWhiteSpace(resultadoFiltro))
            return query;

        var f = resultadoFiltro.Trim();
        if (f.Equals("todos", StringComparison.OrdinalIgnoreCase))
            return query;
        if (f.Equals("sucesso", StringComparison.OrdinalIgnoreCase))
            return query.Where(e => e.Sucesso == true);
        if (f.Equals("falha", StringComparison.OrdinalIgnoreCase))
            return query.Where(e => e.Sucesso == false);
        if (f.Equals("emAndamento", StringComparison.OrdinalIgnoreCase))
            return query.Where(e => e.FimUtc == null);

        return query;
    }

    private static IQueryable<JobExecucaoEntity> ApplyOrdenacao(
        IQueryable<JobExecucaoEntity> query,
        string sortColumn,
        bool sortDescending)
    {
        return sortColumn switch
        {
            "fireInstanceId" => sortDescending
                ? query.OrderByDescending(e => e.FireInstanceId)
                : query.OrderBy(e => e.FireInstanceId),
            "jobNome" => sortDescending ? query.OrderByDescending(e => e.JobNome) : query.OrderBy(e => e.JobNome),
            "jobGrupo" => sortDescending ? query.OrderByDescending(e => e.JobGrupo) : query.OrderBy(e => e.JobGrupo),
            "triggerNome" => sortDescending
                ? query.OrderByDescending(e => e.TriggerNome)
                : query.OrderBy(e => e.TriggerNome),
            "triggerGrupo" => sortDescending
                ? query.OrderByDescending(e => e.TriggerGrupo)
                : query.OrderBy(e => e.TriggerGrupo),
            "fimUtc" => sortDescending ? query.OrderByDescending(e => e.FimUtc) : query.OrderBy(e => e.FimUtc),
            "duracaoMs" => sortDescending
                ? query.OrderByDescending(e => e.DuracaoMs)
                : query.OrderBy(e => e.DuracaoMs),
            "sucesso" => sortDescending ? query.OrderByDescending(e => e.Sucesso) : query.OrderBy(e => e.Sucesso),
            "mensagemErro" => sortDescending
                ? query.OrderByDescending(e => e.MensagemErro)
                : query.OrderBy(e => e.MensagemErro),
            _ => sortDescending ? query.OrderByDescending(e => e.InicioUtc) : query.OrderBy(e => e.InicioUtc)
        };
    }

    private static string? Truncar(string? s, int max)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        return s.Length <= max ? s : s[..max];
    }
}
