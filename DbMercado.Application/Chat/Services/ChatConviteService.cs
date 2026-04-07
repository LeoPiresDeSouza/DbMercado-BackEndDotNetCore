using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DbMercado.Application.Chat.Services;

public sealed class ChatConviteService : IChatConviteService
{
    private readonly IUwChat _uwChat;
    private readonly UserManager<IdentityUser> _userManager;

    public ChatConviteService(IUwChat uwChat, UserManager<IdentityUser> userManager)
    {
        _uwChat = uwChat;
        _userManager = userManager;
    }

    public async Task<RoomResponse> AceitarAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid inviteId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        var convite = await _uwChat.Convites.ObterPorIdComTrackingAsync(inviteId, cancellationToken);
        ValidarConviteParaDestinatario(convite, identityUserId, inviteId);

        var sala = await _uwChat.Salas.ObterPorIdAsync(convite!.RoomId, cancellationToken);
        if (sala is null)
            throw new EntityNotFoundException("Sala de chat", convite.RoomId);

        if (sala.Status != ChatRoomStatus.Active)
            throw new BusinessException("CHAT_SALA_ARQUIVADA", "Não é possível entrar em uma sala arquivada.");

        if (await _uwChat.Membros.ExisteMembroNaSalaAsync(convite.RoomId, identityUserId, cancellationToken))
            throw new BusinessException("CHAT_USUARIO_JA_MEMBRO", "Você já é membro desta sala.");

        var joinedAt = DateTime.UtcNow;
        var membro = new ChatMemberEntity
        {
            RoomId = convite.RoomId,
            UserId = identityUserId,
            LanguagePref = LanguageCode.PtBr,
            Role = ChatMemberRole.Member,
            JoinedAt = joinedAt
        };

        convite.Status = ChatInviteStatus.Accepted;
        await _uwChat.Membros.AdicionarAsync(usuarioAuditoria, membro, cancellationToken);
        await _uwChat.Convites.AtualizarAsync(usuarioAuditoria, convite, cancellationToken);
        await _uwChat.SaveChangesAsync(cancellationToken);

        var salaComMembros = await _uwChat.Salas.ObterPorIdComMembrosAsync(convite.RoomId, cancellationToken);
        if (salaComMembros is null)
            throw new EntityNotFoundException("Sala de chat", convite.RoomId);

        return MapearRoomResponse(salaComMembros, identityUserId);
    }

    public async Task RecusarAsync(
        string identityUserId,
        string usuarioAuditoria,
        Guid inviteId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        var convite = await _uwChat.Convites.ObterPorIdComTrackingAsync(inviteId, cancellationToken);
        ValidarConviteParaDestinatario(convite, identityUserId, inviteId);

        convite!.Status = ChatInviteStatus.Declined;
        await _uwChat.Convites.AtualizarAsync(usuarioAuditoria, convite, cancellationToken);
        await _uwChat.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InviteResponse>> ListarPendentesParaUsuarioAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");
        }

        var convites = await _uwChat.Convites.ListarPendentesNaoExpiradosParaConvidadoAsync(
            identityUserId,
            cancellationToken);

        if (convites.Count == 0)
        {
            return Array.Empty<InviteResponse>();
        }

        var resultado = new List<InviteResponse>(convites.Count);
        foreach (var c in convites)
        {
            var quemConvida = await _userManager.FindByIdAsync(c.InvitedBy);
            var nomeQuemConvida = quemConvida?.UserName
                                  ?? quemConvida?.Email
                                  ?? c.InvitedBy;

            resultado.Add(
                new InviteResponse
                {
                    Id = c.Id,
                    RoomId = c.RoomId,
                    RoomName = c.Room?.Name ?? string.Empty,
                    InvitedByName = nomeQuemConvida,
                    ExpiresAt = c.ExpiresAt
                });
        }

        return resultado;
    }

    private static void ValidarConviteParaDestinatario(
        ChatInviteEntity? convite,
        string identityUserId,
        Guid inviteId)
    {
        if (convite is null)
            throw new EntityNotFoundException("Convite", inviteId);

        if (!string.Equals(convite.InvitedUserId, identityUserId, StringComparison.Ordinal))
            throw new BusinessException(
                "CHAT_CONVITE_DESTINATARIO_INVALIDO",
                "Somente o usuário convidado pode aceitar ou recusar este convite.");

        if (convite.Status != ChatInviteStatus.Pending)
            throw new BusinessException(
                "CHAT_CONVITE_NAO_PENDENTE",
                $"Este convite não está pendente (status atual: {convite.Status}).");

        if (convite.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("CHAT_CONVITE_EXPIRADO", "Este convite expirou.");
    }

    private static RoomResponse MapearRoomResponse(ChatRoomEntity sala, string identityUserId)
    {
        var meuPapel = sala.Members.FirstOrDefault(m => m.UserId == identityUserId);
        if (meuPapel is null)
            throw new BusinessException(
                "CHAT_MEMBRO_POS_ACEITE_AUSENTE",
                "Não foi possível carregar seu vínculo com a sala após aceitar o convite. Tente listar suas salas novamente.");

        return new RoomResponse
        {
            Id = sala.Id,
            Name = sala.Name,
            Description = sala.Description,
            Status = sala.Status.ToString(),
            MemberCount = sala.Members.Count,
            MyRole = meuPapel.Role.ToString(),
            MyLanguagePref = meuPapel.LanguagePref,
            LastMessageAt = null
        };
    }
}
