namespace DbMercado.Application.Produto.Dtos;

/// <summary>
/// Dados logísticos do produto (dimensões, embalagem, peso e unidades).
/// </summary>
public class ProdutoLogisticaResponseDto
{
    public ProdutoDimensaoDto? DimensaoProduto { get; set; }

    public ProdutoDimensaoEmbalagemDto DimensaoEmbalagem { get; set; } = null!;

    public string UnidadeComercializacao { get; set; } = string.Empty;

    public string UnidadeMedidaFisica { get; set; } = string.Empty;

    public string TipoEmbalagem { get; set; } = string.Empty;
}
