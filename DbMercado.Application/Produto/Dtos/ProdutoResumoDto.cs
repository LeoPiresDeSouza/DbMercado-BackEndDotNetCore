namespace DbMercado.Application.Produto.Dtos;

public class ProdutoResumoDto
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeComercializacao { get; set; } = string.Empty;

    public string UnidadeMedidaFisica { get; set; } = string.Empty;

    public string TipoEmbalagem { get; set; } = string.Empty;

    public string? Marca { get; set; }
}
