using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Chat.Dtos;

public sealed class CreateRoomRequest
{
    [Required(ErrorMessage = "O nome da sala é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome da sala deve ter no máximo 120 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
