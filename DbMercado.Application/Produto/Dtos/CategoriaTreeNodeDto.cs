namespace DbMercado.Application.Produto.Dtos;

/// <summary>Nó da árvore de categorias para exibição em selects e painéis de filtro.</summary>
public class CategoriaTreeNodeDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public long? CategoriaPaiId { get; set; }
    public int Nivel { get; set; }
    public bool Ativo { get; set; }
    public IReadOnlyList<CategoriaTreeNodeDto> Subcategorias { get; set; } = Array.Empty<CategoriaTreeNodeDto>();
}
