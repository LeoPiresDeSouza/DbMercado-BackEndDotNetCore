namespace DbMercado.Application.Produto.Dtos;

public class ProdutoResponseDto
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Gtin { get; set; }

    public string UnidadeMedida { get; set; } = string.Empty;

    public string OrigemGeograficaTipo { get; set; } = string.Empty;

    public string? OrigemGeograficaPais { get; set; }

    public ProdutoDadosFiscaisDto DadosFiscais { get; set; } = null!;

    public ProdutoDimensaoDto? DimensaoProduto { get; set; }

    public ProdutoDimensaoEmbalagemDto DimensaoEmbalagem { get; set; } = null!;

    public IReadOnlyList<ProdutoSkuResponseDto> Skus { get; set; } = Array.Empty<ProdutoSkuResponseDto>();

    public IReadOnlyList<ProdutoAtributoDto> Atributos { get; set; } = Array.Empty<ProdutoAtributoDto>();
}
