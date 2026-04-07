using DbMercado.Api.Extensions;
using DbMercado.Api.Filters;
using DbMercado.Application.Administracao;
using DbMercado.Application.Chat.Dtos;
using DbMercado.Application.Chat.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Chat;

/// <summary>
/// Aceite ou recusa de convites de chat.
/// Requer JWT e permissão <c>acessar</c> na funcionalidade <c>chatmultilingue</c>.
/// </summary>
[Authorize]
[ApiController]
[Route("api/chat/invites")]
public sealed class ChatInvitesController : ControllerBase
{
    private readonly IChatConviteService _chatConviteService;

    public ChatInvitesController(IChatConviteService chatConviteService)
    {
        _chatConviteService = chatConviteService;
    }

    private string UsuarioAuditoria => User.ResolveUsuarioAuditoria();

    /// <summary>Lista convites pendentes e não expirados para o usuário autenticado (salas ativas).</summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<InviteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<IReadOnlyList<InviteResponse>>> ListarPendentes(
        CancellationToken cancellationToken)
    {
        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var lista = await _chatConviteService.ListarPendentesParaUsuarioAsync(userId, cancellationToken);
        return Ok(lista);
    }

    /// <summary>Aceita o convite e retorna os dados da sala em que o usuário passou a ser membro.</summary>
    /// <param name="id">Identificador do convite.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<RoomResponse>> Aceitar(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var sala = await _chatConviteService.AceitarAsync(userId, UsuarioAuditoria, id, cancellationToken);
        return Ok(sala);
    }

    /// <summary>Recusa o convite (sem corpo de resposta).</summary>
    /// <param name="id">Identificador do convite.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpPost("{id:guid}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<IActionResult> Recusar(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await _chatConviteService.RecusarAsync(userId, UsuarioAuditoria, id, cancellationToken);
        return NoContent();
    }
}
