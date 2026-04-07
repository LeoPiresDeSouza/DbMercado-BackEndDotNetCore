using System.ComponentModel.DataAnnotations;

namespace DbMercado.Application.Chat.Dtos;

/// <summary>Payload do cliente ao enviar mensagem (SignalR / hub).</summary>
public sealed class SendMessageRequest
{
    public Guid RoomId { get; set; }

    [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório.")]
    [MaxLength(8000, ErrorMessage = "O conteúdo da mensagem deve ter no máximo 8000 caracteres.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "O idioma de origem é obrigatório.")]
    [MaxLength(32, ErrorMessage = "O código de idioma deve ter no máximo 32 caracteres.")]
    public string SourceLang { get; set; } = string.Empty;
}
