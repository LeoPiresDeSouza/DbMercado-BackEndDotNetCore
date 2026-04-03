namespace DbMercado.Application.Produto.Dtos;

/// <summary>
/// Item leve para listagens e consultas (sem estoque, apenas dados mestre do produto).
/// </summary>
public class ProdutoListItemDto
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Gtin { get; set; }

    public string UnidadeComercializacao { get; set; } = string.Empty;

    public string UnidadeMedidaFisica { get; set; } = string.Empty;

    public string TipoEmbalagem { get; set; } = string.Empty;

    public string Ncm { get; set; } = string.Empty;

    public string OrigemGeograficaTipo { get; set; } = string.Empty;

    public string? OrigemGeograficaPais { get; set; }
}
