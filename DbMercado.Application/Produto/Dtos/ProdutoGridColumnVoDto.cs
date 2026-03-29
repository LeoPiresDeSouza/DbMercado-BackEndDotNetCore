namespace DbMercado.Application.Produto.Dtos;

/// <summary>Alinhado ao <c>ColumnVO</c> do AG Grid (SSRM).</summary>
public class ProdutoGridColumnVoDto
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Field { get; set; }

    public string? AggFunc { get; set; }
}
