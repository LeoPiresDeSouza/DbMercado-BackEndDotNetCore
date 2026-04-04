using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DeepBlues.Infrastructure.Providers;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Administracao.Repositories;

public class ParametroChaveConsultaRepository : IParametroChaveConsultaRepository
{
    private readonly AppDbContext _context;
    private readonly IApplicationCachingService<ParametroEntity> _cache;

    public ParametroChaveConsultaRepository(
        AppDbContext context,
        IApplicationCachingService<ParametroEntity> cache)
    {
        _context = context;
        _cache = cache;
    }

    public Task<bool> ExisteChaveAsync(
        string categoria,
        string atributo,
        string chave,
        CancellationToken cancellationToken = default)
    {
        return _context.Parametros.AsNoTracking()
            .AnyAsync(
                p => p.Categoria == categoria && p.Atributo == atributo && p.Chave == chave,
                cancellationToken);
    }

    public Task<IReadOnlyList<(string Chave, string Valor)>> ListarPorCategoriaEAtributoAsync(
        string categoria,
        string atributo,
        CancellationToken cancellationToken = default)
    {
        var key = $"ListarPorCategoriaEAtributo:{categoria}:{atributo}";
        return _cache.GetOrCreateAsync(
            CachePolicy.LongTerm,
            key,
            async () =>
            {
                var rows = await _context.Parametros.AsNoTracking()
                    .Where(p => p.Categoria == categoria && p.Atributo == atributo)
                    .OrderBy(p => p.Chave)
                    .Select(p => new { p.Chave, p.Valor })
                    .ToListAsync(cancellationToken);
                return (IReadOnlyList<(string Chave, string Valor)>)rows.ConvertAll(r => (r.Chave, r.Valor));
            });
    }
}
