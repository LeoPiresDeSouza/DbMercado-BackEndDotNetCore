namespace DbMercado.Application.Produto.Dtos;

/// <summary>Uma linha do grid: produto ou grupo (server-side row grouping).</summary>
public class ProdutoGridRowDto
{
    public bool IsGroup { get; set; }

    public long? Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeComercializacao { get; set; } = string.Empty;

    public string UnidadeMedidaFisica { get; set; } = string.Empty;

    public string TipoEmbalagem { get; set; } = string.Empty;

    public string? Marca { get; set; }

    public string? CategoriaNome { get; set; }

    public string? CategoriaSlug { get; set; }

    /// <summary>Filhos diretos; usado pelo AG Grid via <c>getChildCount</c>.</summary>
    public int ChildCount { get; set; }

    /// <summary>Chave do nível atual (SSRM <c>getServerSideGroupKey</c>).</summary>
    public string GroupKey { get; set; } = string.Empty;
}
