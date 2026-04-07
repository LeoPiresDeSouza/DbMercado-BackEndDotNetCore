using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Chat.Dtos;

public sealed class InviteUserRequest
{
    [Required(ErrorMessage = "O identificador do usuário convidado é obrigatório.")]
    [MaxLength(450, ErrorMessage = "O identificador do usuário deve ter no máximo 450 caracteres.")]
    public string UserId { get; set; } = string.Empty;
}
