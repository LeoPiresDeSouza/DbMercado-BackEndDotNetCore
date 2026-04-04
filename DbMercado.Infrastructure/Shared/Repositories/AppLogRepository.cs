using DbMercado.Domain.Shared.Entities;
using DbMercado.Domain.Shared.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Shared.Repositories;

public sealed class AppLogRepository : IAppLogRepository
{
    private readonly AppDbContext _context;

    public AppLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<AppLogGridConsultaLinha> Rows, int RowCount)> ConsultarGridAsync(
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        bool somenteComExcecao,
        IReadOnlyList<string>? levels,
        int skip,
        int take,
        string sortColumn,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = BuildFilteredQuery(dataInicio, dataFim, somenteComExcecao, levels);

        var total = await baseQuery.CountAsync(cancellationToken);

        var ordered = ApplyOrdenacao(baseQuery, sortColumn, sortDescending);

        var rows = await ordered
            .Skip(skip)
            .Take(take)
            .Select(e => new AppLogGridConsultaLinha
            {
                Id = e.Id,
                CreatedAt = e.CreatedAt,
                Level = e.Level,
                Category = e.Category,
                Message = e.Message,
                HasException = e.Exception != null,
                UserName = e.UserName,
                Path = e.Path,
                Method = e.Method
            })
            .ToListAsync(cancellationToken);

        return (rows, total);
    }

    private IQueryable<AppLogEntity> BuildFilteredQuery(
        DateTimeOffset? dataInicio,
        DateTimeOffset? dataFim,
        bool somenteComExcecao,
        IReadOnlyList<string>? levels)
    {
        var query = _context.AppLogEntries.AsNoTracking();

        if (dataInicio.HasValue)
            query = query.Where(e => e.CreatedAt >= dataInicio.Value);
        if (dataFim.HasValue)
            query = query.Where(e => e.CreatedAt <= dataFim.Value);
        if (somenteComExcecao)
            query = query.Where(e => e.Exception != null);
        if (levels is { Count: > 0 })
        {
            var set = levels.Select(l => l.Trim()).Where(l => l.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (set.Count > 0)
                query = query.Where(e => set.Contains(e.Level));
        }

        return query;
    }

    private static IQueryable<AppLogEntity> ApplyOrdenacao(
        IQueryable<AppLogEntity> query,
        string sortColumn,
        bool sortDescending)
    {
        return sortColumn switch
        {
            "level" => sortDescending ? query.OrderByDescending(e => e.Level) : query.OrderBy(e => e.Level),
            "category" => sortDescending ? query.OrderByDescending(e => e.Category) : query.OrderBy(e => e.Category),
            "message" => sortDescending ? query.OrderByDescending(e => e.Message) : query.OrderBy(e => e.Message),
            "userName" => sortDescending ? query.OrderByDescending(e => e.UserName) : query.OrderBy(e => e.UserName),
            "path" => sortDescending ? query.OrderByDescending(e => e.Path) : query.OrderBy(e => e.Path),
            "method" => sortDescending ? query.OrderByDescending(e => e.Method) : query.OrderBy(e => e.Method),
            _ => sortDescending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt)
        };
    }

    public Task<AppLogEntity?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        _context.AppLogEntries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<bool> ExcluirPorIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var affected = await _context.AppLogEntries.Where(e => e.Id == id).ExecuteDeleteAsync(cancellationToken);
        return affected > 0;
    }

    public Task<int> ContarTotalAsync(CancellationToken cancellationToken = default) =>
        _context.AppLogEntries.AsNoTracking().CountAsync(cancellationToken);

    public async Task<IReadOnlyList<AppLogEntity>> ObterMaisAntigosAsync(int quantidade, CancellationToken cancellationToken = default)
    {
        if (quantidade <= 0)
            return [];

        return await _context.AppLogEntries.AsNoTracking()
            .OrderBy(e => e.CreatedAt)
            .Take(quantidade)
            .ToListAsync(cancellationToken);
    }

    public Task<int> ExcluirPorIdsAsync(IReadOnlyList<long> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return Task.FromResult(0);

        return _context.AppLogEntries
            .Where(e => ids.Contains(e.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
