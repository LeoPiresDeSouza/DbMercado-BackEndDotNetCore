using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.Interfaces;
using DbMercado.Domain.Chat;
using DbMercado.Domain.Chat.Entities;
using DbMercado.Domain.Chat.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace DbMercado.Application.Chat.Services;

public sealed class ChatSalaService : IChatSalaService
{
    private readonly IUwChat _uwChat;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IChatConviteRealtimeNotifier _conviteRealtimeNotifier;

    public ChatSalaService(
        IUwChat uwChat,
        UserManager<IdentityUser> userManager,
        IChatConviteRealtimeNotifier conviteRealtimeNotifier)
    {
        _uwChat = uwChat;
        _userManager = userManager;
        _conviteRealtimeNotifier = conviteRealtimeNotifier;
    }

    public async Task<RoomResponse> CriarSalaAsync(
        string identityUserId,
        string usuarioAuditoria,
        CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        var nome = request.Name.Trim();
        if (nome.Length == 0)
            throw new BusinessException("CHAT_SALA_NOME_OBRIGATORIO", "O nome da sala é obrigatório.");

        var roomId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;

        var room = new ChatRoomEntity
        {
            Id = roomId,
            OwnerId = identityUserId,
            Name = nome,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = ChatRoomStatus.Active
        };

        var member = new ChatMemberEntity
        {
            RoomId = roomId,
            UserId = identityUserId,
            LanguagePref = LanguageCode.PtBr,
            Role = ChatMemberRole.Owner,
            JoinedAt = joinedAt
        };

        await _uwChat.Salas.AdicionarAsync(usuarioAuditoria, room, cancellationToken);
        await _uwChat.Membros.AdicionarAsync(usuarioAuditoria, member, cancellationToken);
        await _uwChat.SaveChangesAsync(cancellationToken);

        return new RoomResponse
        {
            Id = roomId,
            Name = room.Name,
            Description = room.Description,
            Status = room.Status.ToString(),
            MemberCount = 1,
            MyRole = ChatMemberRole.Owner.ToString(),
            MyLanguagePref = LanguageCode.PtBr,
            LastMessageAt = null
        };
    }

    public async Task<IReadOnlyList<RoomResponse>> ListarMinhasSalasAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        var linhas = await _uwChat.Salas.ListarSalasDoUsuarioAsync(identityUserId, cancellationToken);
        return linhas.Select(static l => new RoomResponse
        {
            Id = l.Id,
            Name = l.Name,
            Description = l.Description,
            Status = l.Status.ToString(),
            MemberCount = l.MemberCount,
            MyRole = l.MyRole.ToString(),
            MyLanguagePref = l.MyLanguagePref,
            LastMessageAt = l.LastMessageAt
        }).ToList();
    }

    public async Task<InviteResponse> ConvidarUsuarioAsync(
        string identityUserIdInviter,
        string usuarioAuditoria,
        Guid roomId,
        InviteUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(identityUserIdInviter))
            throw new UnauthorizedAccessException("Usuário não autenticado ou identificador ausente no token.");

        var invitedUserId = request.UserId.Trim();
        if (invitedUserId.Length == 0)
            throw new BusinessException("CHAT_CONVITE_USUARIO_OBRIGATORIO", "O identificador do usuário convidado é obrigatório.");

        if (string.Equals(invitedUserId, identityUserIdInviter, StringComparison.Ordinal))
            throw new BusinessException("CHAT_CONVITE_A_SI_MESMO", "Você não pode convidar a si mesmo.");

        var room = await _uwChat.Salas.ObterPorIdComMembrosAsync(roomId, cancellationToken);
        if (room is null)
            throw new EntityNotFoundException("Sala de chat", roomId);

        if (room.Status != ChatRoomStatus.Active)
            throw new BusinessException("CHAT_SALA_ARQUIVADA", "Não é possível convidar membros para uma sala arquivada.");

        var papelConvidante = room.Members.FirstOrDefault(m => m.UserId == identityUserIdInviter);
        if (papelConvidante is null)
            throw new BusinessException("CHAT_CONVITE_SEM_PERMISSAO", "Apenas membros da sala com papel de proprietário ou administrador podem enviar convites.");

        if (papelConvidante.Role is not ChatMemberRole.Owner and not ChatMemberRole.Admin)
            throw new BusinessException("CHAT_CONVITE_SEM_PERMISSAO", "Apenas membros da sala com papel de proprietário ou administrador podem enviar convites.");

        if (room.Members.Any(m => m.UserId == invitedUserId))
            throw new BusinessException("CHAT_USUARIO_JA_MEMBRO", "Este usuário já é membro da sala.");

        var usuarioAlvo = await _userManager.FindByIdAsync(invitedUserId);
        if (usuarioAlvo is null)
            throw new EntityNotFoundException("Usuário", invitedUserId);

        if (await _uwChat.Convites.ExisteConvitePendenteAsync(roomId, invitedUserId, cancellationToken))
            throw new BusinessException("CHAT_CONVITE_JA_PENDENTE", "Já existe um convite pendente para este usuário nesta sala.");

        var conviteId = Guid.NewGuid();
        var expiraEm = DateTime.UtcNow.AddHours(48);

        var convite = new ChatInviteEntity
        {
            Id = conviteId,
            RoomId = roomId,
            InvitedBy = identityUserIdInviter,
            InvitedUserId = invitedUserId,
            Status = ChatInviteStatus.Pending,
            ExpiresAt = expiraEm
        };

        await _uwChat.Convites.AdicionarAsync(usuarioAuditoria, convite, cancellationToken);
        await _uwChat.SaveChangesAsync(cancellationToken);

        var quemConvida = await _userManager.FindByIdAsync(identityUserIdInviter);
        var nomeQuemConvida = quemConvida?.UserName
                              ?? quemConvida?.Email
                              ?? identityUserIdInviter;

        var resposta = new InviteResponse
        {
            Id = conviteId,
            RoomId = roomId,
            RoomName = room.Name,
            InvitedByName = nomeQuemConvida,
            ExpiresAt = expiraEm
        };

        await _conviteRealtimeNotifier.NotificarUsuarioConvidadoAsync(
            invitedUserId,
            resposta,
            cancellationToken);

        return resposta;
    }
}
