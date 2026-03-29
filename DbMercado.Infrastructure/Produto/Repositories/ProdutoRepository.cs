using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Domain.Produto.Queries;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Produto.Repositories;

public class ProdutoRepository : BaseRepository<ProdutoEntity>, IProdutoRepository
{
    /// <summary>Cache alinhado ao <see cref="RepositoryFactory"/> (mesmo padrão de ProdutoImportadoRepository).</summary>
    public ProdutoRepository(
        AppDbContext context,
        IApplicationCachingService<ProdutoEntity> _) : base(context)
    {
    }

    public async Task<ProdutoEntity?> GetByIdCompletoAsync(long id, bool rastrear = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!rastrear)
            query = query.AsNoTracking();

        query = query
            .Include(p => p.Skus.OrderBy(s => s.Id));

        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<ProdutoEntity>> ListarCatalogoAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorNcmAsync(string ncm, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.DadosFiscais.Ncm == ncm)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorOrigemGeograficaAsync(
        string tipoOrigemCodigo,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.OrigemProduto.Tipo == tipoOrigemCodigo)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorUnidadeMedidaAsync(string unidadeMedida, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.UnidadeMedida.ToUpper() == unidadeMedida.ToUpper())
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorMarcaContendoAsync(string marca, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.Marca != null && p.Marca.ToLower().Contains(marca))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ProdutoEntity> Items, int TotalCount)> ConsultarGridAsync(
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = DbSet.AsNoTracking();
        var filtered = AplicarFiltrosGrid(baseQuery, spec.Filtro);
        var total = await filtered.CountAsync(cancellationToken);
        var ordered = AplicarOrdenacaoGrid(filtered, spec.Ordenacao);
        var items = await ordered
            .Skip(spec.Skip)
            .Take(spec.Take)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    private static IQueryable<ProdutoEntity> AplicarFiltrosGrid(
        IQueryable<ProdutoEntity> query,
        ProdutoGridFiltro filtro)
    {
        query = AplicarFiltrosId(query, filtro.Id);
        query = AplicarFiltrosNome(query, filtro.Nome);
        query = AplicarFiltrosMarca(query, filtro.Marca);
        query = AplicarFiltrosUnidade(query, filtro.UnidadeMedida);
        return query;
    }

    private static IQueryable<ProdutoEntity> AplicarFiltrosId(
        IQueryable<ProdutoEntity> query,
        IReadOnlyList<FiltroNumeroColuna> filtros)
    {
        foreach (var f in filtros)
        {
            query = f.Operador switch
            {
                NumeroFiltroOperador.Igual when f.Valor is { } v => query.Where(p => p.Id == v),
                NumeroFiltroOperador.Diferente when f.Valor is { } v => query.Where(p => p.Id != v),
                NumeroFiltroOperador.Menor when f.Valor is { } v => query.Where(p => p.Id < v),
                NumeroFiltroOperador.MenorOuIgual when f.Valor is { } v => query.Where(p => p.Id <= v),
                NumeroFiltroOperador.Maior when f.Valor is { } v => query.Where(p => p.Id > v),
                NumeroFiltroOperador.MaiorOuIgual when f.Valor is { } v => query.Where(p => p.Id >= v),
                NumeroFiltroOperador.Entre when f.Valor is { } a && f.ValorAte is { } b =>
                    query.Where(p => p.Id >= a && p.Id <= b),
                NumeroFiltroOperador.EmBranco => query.Where(_ => false),
                NumeroFiltroOperador.NaoEmBranco => query,
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<ProdutoEntity> AplicarFiltrosNome(
        IQueryable<ProdutoEntity> query,
        IReadOnlyList<FiltroTextoColuna> filtros)
    {
        foreach (var f in filtros)
        {
            var v = f.Valor ?? string.Empty;
            query = f.Operador switch
            {
                TextoFiltroOperador.Contem when v.Length > 0 =>
                    query.Where(p => p.Nome.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.NaoContem when v.Length > 0 =>
                    query.Where(p => !p.Nome.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.Igual when v.Length > 0 =>
                    query.Where(p => p.Nome.ToLower() == v.ToLower()),
                TextoFiltroOperador.Diferente when v.Length > 0 =>
                    query.Where(p => p.Nome.ToLower() != v.ToLower()),
                TextoFiltroOperador.ComecaCom when v.Length > 0 =>
                    query.Where(p => p.Nome.ToLower().StartsWith(v.ToLower())),
                TextoFiltroOperador.TerminaCom when v.Length > 0 =>
                    query.Where(p => p.Nome.ToLower().EndsWith(v.ToLower())),
                TextoFiltroOperador.EmBranco => query.Where(p => p.Nome == string.Empty),
                TextoFiltroOperador.NaoEmBranco => query.Where(p => p.Nome != string.Empty),
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<ProdutoEntity> AplicarFiltrosMarca(
        IQueryable<ProdutoEntity> query,
        IReadOnlyList<FiltroTextoColuna> filtros)
    {
        foreach (var f in filtros)
        {
            var v = f.Valor ?? string.Empty;
            query = f.Operador switch
            {
                TextoFiltroOperador.Contem when v.Length > 0 =>
                    query.Where(p => p.Marca != null && p.Marca.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.NaoContem when v.Length > 0 =>
                    query.Where(p => p.Marca == null || !p.Marca.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.Igual when v.Length > 0 =>
                    query.Where(p => p.Marca != null && p.Marca.ToLower() == v.ToLower()),
                TextoFiltroOperador.Diferente when v.Length > 0 =>
                    query.Where(p => p.Marca == null || p.Marca.ToLower() != v.ToLower()),
                TextoFiltroOperador.ComecaCom when v.Length > 0 =>
                    query.Where(p => p.Marca != null && p.Marca.ToLower().StartsWith(v.ToLower())),
                TextoFiltroOperador.TerminaCom when v.Length > 0 =>
                    query.Where(p => p.Marca != null && p.Marca.ToLower().EndsWith(v.ToLower())),
                TextoFiltroOperador.EmBranco => query.Where(p => p.Marca == null || p.Marca == string.Empty),
                TextoFiltroOperador.NaoEmBranco => query.Where(p => p.Marca != null && p.Marca != string.Empty),
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<ProdutoEntity> AplicarFiltrosUnidade(
        IQueryable<ProdutoEntity> query,
        IReadOnlyList<FiltroTextoColuna> filtros)
    {
        foreach (var f in filtros)
        {
            var v = f.Valor ?? string.Empty;
            query = f.Operador switch
            {
                TextoFiltroOperador.Contem when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedida.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.NaoContem when v.Length > 0 =>
                    query.Where(p => !p.UnidadeMedida.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.Igual when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedida.ToLower() == v.ToLower()),
                TextoFiltroOperador.Diferente when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedida.ToLower() != v.ToLower()),
                TextoFiltroOperador.ComecaCom when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedida.ToLower().StartsWith(v.ToLower())),
                TextoFiltroOperador.TerminaCom when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedida.ToLower().EndsWith(v.ToLower())),
                TextoFiltroOperador.EmBranco => query.Where(p => p.UnidadeMedida == string.Empty),
                TextoFiltroOperador.NaoEmBranco => query.Where(p => p.UnidadeMedida != string.Empty),
                _ => query
            };
        }

        return query;
    }

    private static IQueryable<ProdutoEntity> AplicarOrdenacaoGrid(
        IQueryable<ProdutoEntity> query,
        IReadOnlyList<ProdutoGridOrdenacao> ordenacao)
    {
        if (ordenacao.Count == 0)
            return query.OrderBy(p => p.Nome).ThenBy(p => p.Id);

        IOrderedQueryable<ProdutoEntity>? ordered = null;
        foreach (var o in ordenacao)
        {
            ordered = o.Campo switch
            {
                "id" when ordered is null =>
                    o.Crescente ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id),
                "id" =>
                    o.Crescente ? ordered!.ThenBy(p => p.Id) : ordered!.ThenByDescending(p => p.Id),
                "nome" when ordered is null =>
                    o.Crescente ? query.OrderBy(p => p.Nome) : query.OrderByDescending(p => p.Nome),
                "nome" =>
                    o.Crescente ? ordered!.ThenBy(p => p.Nome) : ordered!.ThenByDescending(p => p.Nome),
                "marca" when ordered is null =>
                    o.Crescente ? query.OrderBy(p => p.Marca) : query.OrderByDescending(p => p.Marca),
                "marca" =>
                    o.Crescente ? ordered!.ThenBy(p => p.Marca) : ordered!.ThenByDescending(p => p.Marca),
                "unidadeMedida" when ordered is null =>
                    o.Crescente
                        ? query.OrderBy(p => p.UnidadeMedida)
                        : query.OrderByDescending(p => p.UnidadeMedida),
                "unidadeMedida" =>
                    o.Crescente
                        ? ordered!.ThenBy(p => p.UnidadeMedida)
                        : ordered!.ThenByDescending(p => p.UnidadeMedida),
                _ when ordered is null => query.OrderBy(p => p.Nome),
                _ => ordered!
            };
        }

        return (ordered ?? query.OrderBy(p => p.Nome)).ThenBy(p => p.Id);
    }
}
