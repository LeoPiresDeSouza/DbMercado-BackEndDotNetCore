using DbMercado.Application.Chat.Dtos;
using DbMercado.Domain.Chat;

namespace DbMercado.Application.Chat.Interfaces;

public interface IChatMensagemService
{
    /// <summary>Persiste mensagem com <see cref="MessageTranslationStatus.Pending"/> e retorna o DTO para broadcast SignalR.</summary>
    Task<MessageDto> EnviarMensagemAsync(
        string identityUserId,
        string usuarioAuditoria,
        SendMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Registra leitura da mensagem por um membro da sala (inclui o remetente, se for membro). Retorna <see cref="Guid"/> da sala para broadcast SignalR.</summary>
    Task<Guid> MarcarComoLidaAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista mensagens da sala paginadas por <c>SentAt</c> descendente.
    /// <see cref="MessageDto.Content"/> reflete tradução descriptografada para o <c>LanguagePref</c> do membro, com fallback para o texto original.
    /// </summary>
    Task<ChatMessagesPageResponse> ListarMensagensSalaPaginadoAsync(
        string identityUserId,
        Guid roomId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
