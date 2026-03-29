namespace DbMercado.Application.Produto.Dtos;

public class ProdutoOrigemDto
{
    /// <summary>Código de origem geográfica (chave em parâmetros, ex.: NACIONAL, IMPORTADO).</summary>
    public string Tipo { get; set; } = string.Empty;

    public string? PaisOrigem { get; set; }
}
