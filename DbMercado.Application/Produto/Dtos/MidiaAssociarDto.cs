using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Produto.Dtos;

public sealed class MidiaAssociarDto
{
    [Required]
    [MinLength(1)]
    public List<MidiaAssociarItemDto> Itens { get; set; } = [];
}

public sealed class MidiaAssociarItemDto
{
    [Range(1, long.MaxValue)]
    public long MidiaId { get; set; }

    public bool IsPrincipal { get; set; }
}
