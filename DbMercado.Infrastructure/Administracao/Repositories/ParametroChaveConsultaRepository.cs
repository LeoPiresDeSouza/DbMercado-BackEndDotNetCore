using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Administracao.Repositories;

public class ParametroChaveConsultaRepository : IParametroChaveConsultaRepository
{
    private readonly AppDbContext _context;

    public ParametroChaveConsultaRepository(AppDbContext context)
    {
        _context = context;
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

    public async Task<IReadOnlyList<(string Chave, string Valor)>> ListarPorCategoriaEAtributoAsync(
        string categoria,
        string atributo,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Parametros.AsNoTracking()
            .Where(p => p.Categoria == categoria && p.Atributo == atributo)
            .OrderBy(p => p.Chave)
            .Select(p => new { p.Chave, p.Valor })
            .ToListAsync(cancellationToken);
        return rows.ConvertAll(r => (r.Chave, r.Valor));
    }
}
