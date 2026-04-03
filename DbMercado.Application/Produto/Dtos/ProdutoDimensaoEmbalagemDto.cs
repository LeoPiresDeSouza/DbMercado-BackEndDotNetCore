namespace DbMercado.Application.Produto.Dtos;

public class ProdutoDimensaoEmbalagemDto
{
    public decimal Altura { get; set; }

    public decimal Largura { get; set; }

    public decimal Comprimento { get; set; }

    public decimal Peso { get; set; }

    /// <summary>Código do parâmetro unidadeDimensao (ex.: CM, M, MM).</summary>
    public string UnidadeDimensao { get; set; } = string.Empty;

    /// <summary>Código do parâmetro unidadePeso (ex.: KG, G, T).</summary>
    public string UnidadePeso { get; set; } = string.Empty;
}
