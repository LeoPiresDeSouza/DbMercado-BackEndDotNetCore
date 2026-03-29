namespace DbMercado.Application.Produto.Dtos;

public class ProdutoDadosFiscaisDto
{
    public string Ncm { get; set; } = string.Empty;

    public string? Cest { get; set; }

    /// <summary>Código de origem ICMS (chave em parâmetros, ex.: 0, 1, 2).</summary>
    public string Origem { get; set; } = string.Empty;
}
