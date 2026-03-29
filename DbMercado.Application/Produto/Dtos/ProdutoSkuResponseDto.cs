namespace DbMercado.Application.Produto.Dtos;

public class ProdutoSkuResponseDto
{
    public long Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public bool Ativo { get; set; }
}
