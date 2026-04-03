namespace DbMercado.Application.Produto.Dtos;

public class CategoriaCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    /// <summary>Nulo para criar categoria raiz.</summary>
    public long? CategoriaPaiId { get; set; }
}
