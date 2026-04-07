using DbMercado.Application.Chat.Dtos;

namespace DbMercado.Application.Chat.Interfaces;

public interface IChatSalaService
{
    /// <param name="identityUserId">ID do ASP.NET Identity (<c>ClaimTypes.NameIdentifier</c> / <c>sub</c>).</param>
    /// <param name="usuarioAuditoria">Texto para campos de auditoria (<c>UsuarioCriacao</c> etc.).</param>
    Task<RoomResponse> CriarSalaAsync(
        string identityUserId,
        string usuarioAuditoria,
        CreateRoomRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Salas em que o usuário possui registro em <c>ChatMembers</c>.</summary>
    Task<IReadOnlyList<RoomResponse>> ListarMinhasSalasAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Cria convite pendente (48h). Apenas membro com papel Owner ou Admin.</summary>
    Task<InviteResponse> ConvidarUsuarioAsync(
        string identityUserIdInviter,
        string usuarioAuditoria,
        Guid roomId,
        InviteUserRequest request,
        CancellationToken cancellationToken = default);
}
