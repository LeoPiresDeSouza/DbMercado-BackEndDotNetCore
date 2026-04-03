namespace DbMercado.Application.Produto.Dtos;

public class ProdutoOrigemDto
{
    /// <summary>Código de origem geográfica (chave em parâmetros, ex.: 1, 2).</summary>
    public string Tipo { get; set; } = string.Empty;

    public string? PaisOrigem { get; set; }
}
