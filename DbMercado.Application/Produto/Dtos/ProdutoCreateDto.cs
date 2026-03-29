namespace DbMercado.Application.Produto.Dtos;

public class ProdutoCreateDto
{
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Gtin { get; set; }

    public string UnidadeMedida { get; set; } = string.Empty;

    public ProdutoOrigemDto OrigemGeografica { get; set; } = null!;

    public ProdutoDadosFiscaisDto DadosFiscais { get; set; } = null!;

    public ProdutoDimensaoDto? DimensaoProduto { get; set; }

    public ProdutoDimensaoEmbalagemDto DimensaoEmbalagem { get; set; } = null!;

    public IReadOnlyList<ProdutoSkuItemDto> Skus { get; set; } = Array.Empty<ProdutoSkuItemDto>();

    public IReadOnlyList<ProdutoAtributoDto>? Atributos { get; set; }
}
