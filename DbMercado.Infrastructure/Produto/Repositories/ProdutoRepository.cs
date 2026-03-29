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

    public async Task<(List<ProdutoGridLinhaConsulta> Items, int TotalCount)> ConsultarGridAsync(
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = DbSet.AsNoTracking();
        var filtered = AplicarFiltrosGrid(baseQuery, spec.Filtro);

        if (spec.CamposAgrupamento.Count > 0)
            return await ConsultarGridAgrupadoAsync(filtered, spec, cancellationToken);

        var total = await filtered.CountAsync(cancellationToken);
        var ordered = AplicarOrdenacaoGrid(filtered, spec.Ordenacao);
        var entities = await ordered
            .Skip(spec.Skip)
            .Take(spec.Take)
            .ToListAsync(cancellationToken);
        var linhasPlana = entities
            .Select(p => new ProdutoGridLinhaConsulta
            {
                LinhaDeGrupo = false,
                Produto = p,
                ValoresAgrupamento = new Dictionary<string, string>(StringComparer.Ordinal),
                ContagemFilhosDiretos = 0,
                ChaveNivelAtual = string.Empty
            })
            .ToList();
        return (linhasPlana, total);
    }

    private async Task<(List<ProdutoGridLinhaConsulta> Items, int TotalCount)> ConsultarGridAgrupadoAsync(
        IQueryable<ProdutoEntity> filtered,
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken)
    {
        var campos = spec.CamposAgrupamento;
        var keys = spec.ChavesGrupo.ToList();
        if (keys.Count > campos.Count)
            keys = keys.Take(campos.Count).ToList();

        var filteredComChaves = AplicarFiltroChavesGrupo(filtered, campos, keys);

        var depth = keys.Count;
        if (depth < campos.Count)
        {
            var campoNivel = campos[depth];
            return await ListarNivelGrupoAsync(
                filteredComChaves,
                campos,
                keys,
                campoNivel,
                spec,
                cancellationToken);
        }

        var totalFolhas = await filteredComChaves.CountAsync(cancellationToken);
        var ordenadoFolhas = AplicarOrdenacaoGrid(filteredComChaves, spec.Ordenacao);
        var folhas = await ordenadoFolhas
            .Skip(spec.Skip)
            .Take(spec.Take)
            .ToListAsync(cancellationToken);
        var linhasFolha = folhas
            .Select(p => new ProdutoGridLinhaConsulta
            {
                LinhaDeGrupo = false,
                Produto = p,
                ValoresAgrupamento = new Dictionary<string, string>(StringComparer.Ordinal),
                ContagemFilhosDiretos = 0,
                ChaveNivelAtual = string.Empty
            })
            .ToList();
        return (linhasFolha, totalFolhas);
    }

    private static IQueryable<ProdutoEntity> AplicarFiltroChavesGrupo(
        IQueryable<ProdutoEntity> query,
        IReadOnlyList<string> camposAgrupamento,
        IReadOnlyList<string> chavesGrupo)
    {
        for (var i = 0; i < chavesGrupo.Count && i < camposAgrupamento.Count; i++)
        {
            var campo = camposAgrupamento[i];
            var chave = chavesGrupo[i] ?? string.Empty;
            query = campo switch
            {
                "marca" => query.Where(p => (p.Marca ?? string.Empty) == chave),
                "nome" => query.Where(p => p.Nome == chave),
                "unidadeMedida" => query.Where(p => p.UnidadeMedida == chave),
                _ => query
            };
        }

        return query;
    }

    private async Task<(List<ProdutoGridLinhaConsulta> Items, int TotalCount)> ListarNivelGrupoAsync(
        IQueryable<ProdutoEntity> filtered,
        IReadOnlyList<string> campos,
        IReadOnlyList<string> chavesPai,
        string campoNivel,
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken)
    {
        return campoNivel switch
        {
            "marca" => await ListarGrupoPorExpressao(
                filtered,
                p => p.Marca ?? string.Empty,
                campos,
                chavesPai,
                "marca",
                spec,
                cancellationToken),
            "nome" => await ListarGrupoPorExpressao(
                filtered,
                p => p.Nome,
                campos,
                chavesPai,
                "nome",
                spec,
                cancellationToken),
            "unidadeMedida" => await ListarGrupoPorExpressao(
                filtered,
                p => p.UnidadeMedida,
                campos,
                chavesPai,
                "unidadeMedida",
                spec,
                cancellationToken),
            _ => (new List<ProdutoGridLinhaConsulta>(), 0)
        };
    }

    private async Task<(List<ProdutoGridLinhaConsulta> Items, int TotalCount)> ListarGrupoPorExpressao(
        IQueryable<ProdutoEntity> filtered,
        System.Linq.Expressions.Expression<Func<ProdutoEntity, string>> selectorKey,
        IReadOnlyList<string> campos,
        IReadOnlyList<string> chavesPai,
        string campoNivel,
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken)
    {
        var agrupado = filtered.GroupBy(selectorKey);
        var total = await agrupado.CountAsync(cancellationToken);
        // Projeção anônima: EF Core traduz para GROUP BY + COUNT; tipo nomeado quebrava a tradução.
        var projetado = agrupado.Select(g => new { Key = g.Key, Cnt = g.Count() });
        var ord0 = spec.Ordenacao.FirstOrDefault();
        var ordenado =
            ord0 != null && string.Equals(ord0.Campo, campoNivel, StringComparison.OrdinalIgnoreCase)
                ? (ord0.Crescente
                    ? projetado.OrderBy(x => x.Key)
                    : projetado.OrderByDescending(x => x.Key))
                : projetado.OrderBy(x => x.Key);

        var pagina = await ordenado
            .Skip(spec.Skip)
            .Take(spec.Take)
            .ToListAsync(cancellationToken);

        var linhas = new List<ProdutoGridLinhaConsulta>(pagina.Count);
        foreach (var item in pagina)
        {
            var valores = MontarValoresAgrupamento(campos, chavesPai, campoNivel, item.Key);
            linhas.Add(new ProdutoGridLinhaConsulta
            {
                LinhaDeGrupo = true,
                Produto = null,
                ValoresAgrupamento = valores,
                ContagemFilhosDiretos = item.Cnt,
                ChaveNivelAtual = item.Key
            });
        }

        return (linhas, total);
    }

    private static Dictionary<string, string> MontarValoresAgrupamento(
        IReadOnlyList<string> campos,
        IReadOnlyList<string> chavesPai,
        string campoAtual,
        string chaveAtual)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < chavesPai.Count && i < campos.Count; i++)
            d[campos[i]] = chavesPai[i] ?? string.Empty;
        d[campoAtual] = chaveAtual;
        return d;
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
