using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Chat.Dtos;

public sealed class LanguageUpdateRequest
{
    [Required(ErrorMessage = "O idioma preferido é obrigatório.")]
    [MaxLength(32, ErrorMessage = "O código de idioma deve ter no máximo 32 caracteres.")]
    public string LanguagePref { get; set; } = string.Empty;
}
