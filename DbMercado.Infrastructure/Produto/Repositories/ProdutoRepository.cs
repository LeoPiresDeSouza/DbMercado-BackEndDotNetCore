using DbMercado.Domain.Produto.Constants;
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
            .Include(p => p.Skus.OrderBy(s => s.Id))
            .Include(p => p.CategoriaProduto)
                .ThenInclude(c => c!.CategoriaPai)
                .ThenInclude(c => c!.CategoriaPai)
                .ThenInclude(c => c!.CategoriaPai);

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
        var tipo12 = NormalizarTipoOrigemPainel(tipoOrigemCodigo);
        if (tipo12 is null)
            return Task.FromResult(new List<ProdutoEntity>());

        var query = DbSet.AsNoTracking();
        query = AplicarFiltroOrigemGeograficaCompativel(query, tipo12);
        return query.OrderBy(p => p.Nome).ToListAsync(cancellationToken);
    }

    public Task<List<ProdutoEntity>> BuscarPorUnidadeMedidaAsync(string unidadeMedida, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(p => p.UnidadeMedidaFisica.ToUpper() == unidadeMedida.ToUpper())
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
        filtered = await AplicarFiltrosPainelAsync(filtered, spec, cancellationToken);

        if (spec.CamposAgrupamento.Count > 0)
            return await ConsultarGridAgrupadoAsync(filtered, spec, cancellationToken);

        var total = await filtered.CountAsync(cancellationToken);
        var ordered = AplicarOrdenacaoGrid(filtered, spec.Ordenacao);
        var entities = await ordered
            .Include(p => p.CategoriaProduto)
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
            .Include(p => p.CategoriaProduto)
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
                "unidadeMedida" => query.Where(p => p.UnidadeMedidaFisica == chave),
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
                p => p.UnidadeMedidaFisica,
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

    private async Task<IQueryable<ProdutoEntity>> AplicarFiltrosPainelAsync(
        IQueryable<ProdutoEntity> query,
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken)
    {
        if (spec.CategoriaIdFiltro is > 0)
        {
            var idsCategoria = await ColetarIdsCategoriasDescendentesAsync(
                spec.CategoriaIdFiltro.Value,
                cancellationToken);
            query = query.Where(p =>
                p.CategoriaProdutoId.HasValue &&
                idsCategoria.Contains(p.CategoriaProdutoId.Value));
        }

        var tipoOrigem = NormalizarTipoOrigemPainel(spec.OrigemFiltro);
        if (tipoOrigem is not null)
            query = AplicarFiltroOrigemGeograficaCompativel(query, tipoOrigem);

        return query;
    }

    private async Task<HashSet<long>> ColetarIdsCategoriasDescendentesAsync(
        long categoriaRaizId,
        CancellationToken cancellationToken)
    {
        var ids = new HashSet<long> { categoriaRaizId };
        var fila = new Queue<long>();
        fila.Enqueue(categoriaRaizId);

        while (fila.Count > 0)
        {
            var atual = fila.Dequeue();
            var filhas = await _context.CategoriasProduto
                .AsNoTracking()
                .Where(c => c.CategoriaPaiId == atual)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var id in filhas)
            {
                if (ids.Add(id))
                    fila.Enqueue(id);
            }
        }

        return ids;
    }

    /// <summary>
    /// Filtra por tipo de origem aceitando armazenamento como código (<c>1</c>/<c>2</c>, pós-domain) ou rótulo legado
    /// (<c>NACIONAL</c>/<c>IMPORTADO</c>) da migração <c>ProdutoParametrosEmVezDeEnums</c>.
    /// </summary>
    private static IQueryable<ProdutoEntity> AplicarFiltroOrigemGeograficaCompativel(
        IQueryable<ProdutoEntity> query,
        string tipoNormalizado12)
    {
        return tipoNormalizado12 switch
        {
            OrigemGeograficaProdutoCodigos.Nacional => query.Where(p =>
                p.OrigemProduto.Tipo == OrigemGeograficaProdutoCodigos.Nacional
                || p.OrigemProduto.Tipo == "NACIONAL"),
            OrigemGeograficaProdutoCodigos.Importado => query.Where(p =>
                p.OrigemProduto.Tipo == OrigemGeograficaProdutoCodigos.Importado
                || p.OrigemProduto.Tipo == "IMPORTADO"),
            _ => query
        };
    }

    /// <summary>Converte rótulos da UI para <c>1</c>/<c>2</c> (comparação ao valor persistido via <see cref="AplicarFiltroOrigemGeograficaCompativel"/>).</summary>
    private static string? NormalizarTipoOrigemPainel(string? origemFiltro)
    {
        if (string.IsNullOrWhiteSpace(origemFiltro))
            return null;

        return origemFiltro.Trim().ToUpperInvariant() switch
        {
            "NACIONAL" => "1",
            "IMPORTADO" => "2",
            "1" => "1",
            "2" => "2",
            _ => null
        };
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
                    query.Where(p => p.UnidadeMedidaFisica.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.NaoContem when v.Length > 0 =>
                    query.Where(p => !p.UnidadeMedidaFisica.ToLower().Contains(v.ToLower())),
                TextoFiltroOperador.Igual when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedidaFisica.ToLower() == v.ToLower()),
                TextoFiltroOperador.Diferente when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedidaFisica.ToLower() != v.ToLower()),
                TextoFiltroOperador.ComecaCom when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedidaFisica.ToLower().StartsWith(v.ToLower())),
                TextoFiltroOperador.TerminaCom when v.Length > 0 =>
                    query.Where(p => p.UnidadeMedidaFisica.ToLower().EndsWith(v.ToLower())),
                TextoFiltroOperador.EmBranco => query.Where(p => p.UnidadeMedidaFisica == string.Empty),
                TextoFiltroOperador.NaoEmBranco => query.Where(p => p.UnidadeMedidaFisica != string.Empty),
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
                        ? query.OrderBy(p => p.UnidadeMedidaFisica)
                        : query.OrderByDescending(p => p.UnidadeMedidaFisica),
                "unidadeMedida" =>
                    o.Crescente
                        ? ordered!.ThenBy(p => p.UnidadeMedidaFisica)
                        : ordered!.ThenByDescending(p => p.UnidadeMedidaFisica),
                _ when ordered is null => query.OrderBy(p => p.Nome),
                _ => ordered!
            };
        }

        return (ordered ?? query.OrderBy(p => p.Nome)).ThenBy(p => p.Id);
    }
}
