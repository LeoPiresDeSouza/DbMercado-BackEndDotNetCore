namespace DbMercado.Application.Produto.Dtos;

/// <summary>
/// Dados logísticos do produto (dimensões, embalagem, peso e unidade de medida).
/// </summary>
public class ProdutoLogisticaResponseDto
{
    public ProdutoDimensaoDto? DimensaoProduto { get; set; }

    public ProdutoDimensaoEmbalagemDto DimensaoEmbalagem { get; set; } = null!;

    public string UnidadeMedida { get; set; } = string.Empty;
}
