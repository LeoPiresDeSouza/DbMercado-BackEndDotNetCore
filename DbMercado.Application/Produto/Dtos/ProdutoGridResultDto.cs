namespace DbMercado.Application.Produto.Dtos;

public class ProdutoGridResultDto
{
    public List<ProdutoGridRowDto> Rows { get; set; } = new();

    /// <summary>Total de linhas após filtros (para lastRow do infinite model).</summary>
    public int RowCount { get; set; }
}
