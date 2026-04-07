using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DbMercado.Application.Chat.Services;

public sealed class ChatMensagemService : IChatMensagemService
{
    private const int ConteudoMaxCaracteres = 8000;
    private const int MensagensPaginaMax = 100;

    private readonly IUwChat _uwChat;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatMensagemService(IUwChat uwChat, UserManager<IdentityUser> userManager)
    {
        _uwChat = uwChat;
        _userManager = userManager;
    }

    public async Task<MessageDto> EnviarMensagemAsync(
        string identityUserId,
        string usuarioAuditoria,
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        var conteudo = request.Content.Trim();
        if (conteudo.Length == 0)
            throw new BusinessException(
                "CHAT_MENSAGEM_CONTEUDO_OBRIGATORIO",
                "O conteúdo da mensagem é obrigatório.");

        if (conteudo.Length > ConteudoMaxCaracteres)
            throw new BusinessException(
                "CHAT_MENSAGEM_CONTEUDO_MAX",
                $"O conteúdo da mensagem deve ter no máximo {ConteudoMaxCaracteres} caracteres.");

        if (!LanguageCode.TryNormalize(request.SourceLang, out var sourceLang))
            throw new BusinessException(
                "CHAT_IDIOMA_INVALIDO",
                "Idioma inválido. Utilize pt-BR, en ou zh-CN.");

        if (request.RoomId == Guid.Empty)
            throw new BusinessException(
                "CHAT_SALA_INVALIDA",
                "Identificador da sala é obrigatório.");

        var sala = await _uwChat.Salas.ObterPorIdAsync(request.RoomId, cancellationToken);
        if (sala is null)
            throw new EntityNotFoundException("Sala de chat", request.RoomId);

        if (sala.Status != ChatRoomStatus.Active)
            throw new BusinessException(
                "CHAT_SALA_ARQUIVADA",
                "Não é possível enviar mensagens em uma sala arquivada.");

        var ehMembro = await _uwChat.Membros.ExisteMembroNaSalaAsync(
            request.RoomId,
            identityUserId,
            cancellationToken);

        if (!ehMembro)
            throw new BusinessException(
                "CHAT_NAO_MEMBRO_SALA",
                "Você não é membro desta sala.");

        var messageId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        var entity = new MessageEntity
        {
            Id = messageId,
            RoomId = request.RoomId,
            SenderId = identityUserId,
            ContentOriginal = conteudo,
            SourceLang = sourceLang,
            MessageType = MessageType.Text,
            TranslationStatus = MessageTranslationStatus.Pending,
            SentAt = sentAt
        };

        await _uwChat.Mensagens.AdicionarAsync(usuarioAuditoria, entity, cancellationToken);
        await _uwChat.SaveChangesAsync(cancellationToken);

        var remetente = await _userManager.FindByIdAsync(identityUserId);
        var senderName = remetente?.UserName
            ?? remetente?.Email
            ?? identityUserId;

        return new MessageDto
        {
            MessageId = messageId,
            RoomId = request.RoomId,
            SenderId = identityUserId,
            SenderName = senderName,
            Content = conteudo,
            IsTranslated = false,
            TranslationStatus = MessageTranslationStatus.Pending,
            SourceLang = sourceLang,
            SentAt = sentAt
        };
    }

    public async Task<Guid> MarcarComoLidaAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        if (messageId == Guid.Empty)
            throw new BusinessException(
                "CHAT_MENSAGEM_INVALIDA",
                "Identificador da mensagem é obrigatório.");

        var mensagem = await _uwChat.Mensagens.ObterPorIdAsync(messageId, cancellationToken);
        if (mensagem is null)
            throw new EntityNotFoundException("Mensagem de chat", messageId);

        if (mensagem.DeletedAt is not null)
            throw new BusinessException(
                "CHAT_MENSAGEM_REMOVIDA",
                "Não é possível marcar leitura de uma mensagem removida.");

        var sala = await _uwChat.Salas.ObterPorIdAsync(mensagem.RoomId, cancellationToken);
        if (sala is null)
            throw new EntityNotFoundException("Sala de chat", mensagem.RoomId);

        if (sala.Status != ChatRoomStatus.Active)
            throw new BusinessException(
                "CHAT_SALA_ARQUIVADA",
                "Não é possível atualizar leitura em uma sala arquivada.");

        var ehMembro = await _uwChat.Membros.ExisteMembroNaSalaAsync(
            mensagem.RoomId,
            identityUserId,
            cancellationToken);

        if (!ehMembro)
            throw new BusinessException(
                "CHAT_NAO_MEMBRO_SALA",
                "Você não é membro desta sala.");

        var readAtUtc = DateTime.UtcNow;
        await _uwChat.Mensagens.RegistrarOuAtualizarLeituraAsync(
            usuarioAuditoria,
            messageId,
            identityUserId,
            readAtUtc,
            cancellationToken);

        await _uwChat.SaveChangesAsync(cancellationToken);

        return mensagem.RoomId;
    }

    public async Task<ChatMessagesPageResponse> ListarMensagensSalaPaginadoAsync(
        string identityUserId,
        Guid roomId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        if (roomId == Guid.Empty)
            throw new BusinessException(
                "CHAT_SALA_INVALIDA",
                "Identificador da sala é obrigatório.");

        if (page < 1)
            throw new BusinessException(
                "CHAT_PAGINA_INVALIDA",
                "O número da página deve ser maior ou igual a 1.");

        if (pageSize < 1 || pageSize > MensagensPaginaMax)
            throw new BusinessException(
                "CHAT_TAMANHO_PAGINA_INVALIDO",
                $"O tamanho da página deve estar entre 1 e {MensagensPaginaMax}.");

        var sala = await _uwChat.Salas.ObterPorIdAsync(roomId, cancellationToken);
        if (sala is null)
            throw new EntityNotFoundException("Sala de chat", roomId);

        var ehMembro = await _uwChat.Membros.ExisteMembroNaSalaAsync(
            roomId,
            identityUserId,
            cancellationToken);

        if (!ehMembro)
            throw new BusinessException(
                "CHAT_NAO_MEMBRO_SALA",
                "Você não é membro desta sala.");

        var prefBruto = await _uwChat.Membros.ObterLanguagePrefMembroAsync(
            roomId,
            identityUserId,
            cancellationToken);

        var idiomaVisualizador = LanguageCode.TryNormalize(prefBruto, out var canônico)
            ? canônico
            : LanguageCode.PtBr;

        var skip = (page - 1) * pageSize;
        var (itens, total) = await _uwChat.Mensagens.ListarPorSalaPaginadoAsync(
            roomId,
            skip,
            pageSize,
            cancellationToken);

        var nomesRemetentes = await ResolverNomesRemetentesAsync(itens, cancellationToken);

        var dtos = new List<MessageDto>(itens.Count);
        foreach (var msg in itens)
        {
            var (content, isTranslated) = ResolverConteudoParaVisualizador(msg, idiomaVisualizador);
            nomesRemetentes.TryGetValue(msg.SenderId, out var senderName);
            dtos.Add(new MessageDto
            {
                MessageId = msg.Id,
                RoomId = msg.RoomId,
                SenderId = msg.SenderId,
                SenderName = senderName ?? msg.SenderId,
                Content = content,
                IsTranslated = isTranslated,
                TranslationStatus = msg.TranslationStatus,
                SourceLang = msg.SourceLang,
                SentAt = msg.SentAt
            });
        }

        return new ChatMessagesPageResponse
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    private async Task<Dictionary<string, string>> ResolverNomesRemetentesAsync(
        IReadOnlyList<MessageEntity> mensagens,
        CancellationToken cancellationToken)
    {
        var ids = mensagens
            .Select(m => m.SenderId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var id in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var u = await _userManager.FindByIdAsync(id);
            map[id] = u?.UserName ?? u?.Email ?? id;
        }

        return map;
    }

    private static (string Content, bool IsTranslated) ResolverConteudoParaVisualizador(
        MessageEntity msg,
        string viewerLangCanonical)
    {
        if (string.Equals(msg.SourceLang, viewerLangCanonical, StringComparison.OrdinalIgnoreCase))
            return (msg.ContentOriginal, false);

        var tr = msg.Translations.FirstOrDefault(t =>
            string.Equals(t.TargetLang, viewerLangCanonical, StringComparison.OrdinalIgnoreCase));

        if (tr is not null && !string.IsNullOrWhiteSpace(tr.TranslatedText))
            return (tr.TranslatedText, true);

        return (msg.ContentOriginal, false);
    }
}
