using DbMercado.Domain.Produto.Entities;

namespace DbMercado.Domain.Produto.Queries;

/// <summary>Linha retornada pela consulta de grid: produto (folha) ou grupo (SSRM / agrupamento).</summary>
public sealed class ProdutoGridLinhaConsulta
{
    public bool LinhaDeGrupo { get; init; }

    public ProdutoEntity? Produto { get; init; }

    /// <summary>Valores das colunas de agrupamento até o nível atual (chaves dos grupos).</summary>
    public IReadOnlyDictionary<string, string> ValoresAgrupamento { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Número de filhos diretos (subgrupos ou folhas) sob este grupo.</summary>
    public int ContagemFilhosDiretos { get; init; }

    /// <summary>Chave deste nó no nível atual (vazio em folhas).</summary>
    public string ChaveNivelAtual { get; init; } = string.Empty;
}
