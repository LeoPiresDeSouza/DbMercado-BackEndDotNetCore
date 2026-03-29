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
}
