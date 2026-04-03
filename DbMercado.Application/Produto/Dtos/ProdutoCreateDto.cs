namespace DbMercado.Application.Produto.Dtos;

public class ProdutoCreateDto
{
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Gtin { get; set; }

    /// <summary>Categoria do produto (opcional).</summary>
    public long? CategoriaProdutoId { get; set; }

    /// <summary>Como o produto é vendido (código do parâmetro unidadeComercializacao).</summary>
    public string UnidadeComercializacao { get; set; } = string.Empty;

    /// <summary>Natureza física para NF-e (código do parâmetro unidadeMedida).</summary>
    public string UnidadeMedidaFisica { get; set; } = string.Empty;

    /// <summary>Tipo de acondicionamento (código do parâmetro unidadeEmbalagem).</summary>
    public string TipoEmbalagem { get; set; } = string.Empty;

    public ProdutoOrigemDto OrigemGeografica { get; set; } = null!;

    public ProdutoDadosFiscaisDto DadosFiscais { get; set; } = null!;

    public ProdutoDimensaoDto? DimensaoProduto { get; set; }

    public ProdutoDimensaoEmbalagemDto DimensaoEmbalagem { get; set; } = null!;

    public IReadOnlyList<ProdutoSkuItemDto> Skus { get; set; } = Array.Empty<ProdutoSkuItemDto>();

    public IReadOnlyList<ProdutoAtributoDto>? Atributos { get; set; }
}
