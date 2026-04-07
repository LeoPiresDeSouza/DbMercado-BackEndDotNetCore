using DbMercado.Application.Chat;
using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Application.Chat.Services;

public sealed class ChatMembroService : IChatMembroService
{
    private readonly IUwChat _uwChat;

    public ChatMembroService(IUwChat uwChat)
    {
        _uwChat = uwChat;
    }

    public async Task ValidarMembroSalaAtivaAsync(
        string identityUserId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        if (roomId == Guid.Empty)
            throw new BusinessException(
                "CHAT_SALA_INVALIDA",
                "Identificador da sala é obrigatório.");

        var sala = await _uwChat.Salas.ObterPorIdAsync(roomId, cancellationToken);
        if (sala is null)
            throw new EntityNotFoundException("Sala de chat", roomId);

        if (sala.Status != ChatRoomStatus.Active)
            throw new BusinessException(
                "CHAT_SALA_ARQUIVADA",
                "Não é possível usar o chat em uma sala arquivada.");

        var ehMembro = await _uwChat.Membros.ExisteMembroNaSalaAsync(
            roomId,
            identityUserId,
            cancellationToken);

        if (!ehMembro)
            throw new BusinessException(
                "CHAT_NAO_MEMBRO_SALA",
                "Você não é membro desta sala.");
    }

    public async Task AtualizarIdiomaPreferidoAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid roomId,
        LanguageUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        if (!LanguageCode.TryNormalize(request.LanguagePref, out var idioma))
            throw new BusinessException(
                "CHAT_IDIOMA_INVALIDO",
                "Idioma inválido. Utilize pt-BR, en ou zh-CN.");

        var membro = await _uwChat.Membros.ObterPorSalaEUsuarioComTrackingAsync(
            roomId,
            identityUserId,
            cancellationToken);

        if (membro is null)
            throw new BusinessException(
                "CHAT_NAO_MEMBRO_SALA",
                "Você não é membro desta sala.");

        membro.LanguagePref = idioma;
        await _uwChat.Membros.AtualizarAsync(usuarioAuditoria, membro, cancellationToken);
        await _uwChat.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<string>> ObterIdiomasAlvoTraducaoAsync(
        Guid roomId,
        string sourceLang,
        CancellationToken cancellationToken = default) =>
        ChatTranslationTargets.ResolverParaSalaAsync(_uwChat, roomId, sourceLang, cancellationToken);
}
