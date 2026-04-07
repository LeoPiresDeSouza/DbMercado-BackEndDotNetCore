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
/// Preferências do membro na sala (idioma da conversa).
/// Requer JWT e permissão <c>acessar</c> na funcionalidade <c>chatmultilingue</c>.
/// </summary>
[Authorize]
[ApiController]
[Route("api/chat/members")]
public sealed class ChatMembersController : ControllerBase
{
    private readonly IChatMembroService _chatMembroService;

    public ChatMembersController(IChatMembroService chatMembroService)
    {
        _chatMembroService = chatMembroService;
    }

    private string UsuarioAuditoria => User.ResolveUsuarioAuditoria();

    /// <summary>Atualiza o idioma preferido do usuário autenticado na sala (ex.: pt-BR, en, zh-CN).</summary>
    /// <param name="roomId">Identificador da sala.</param>
    /// <param name="request">Novo código de idioma (BCP-47 suportado pelo domínio).</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpPut("{roomId:guid}/language")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<IActionResult> AtualizarIdioma(
        [FromRoute] Guid roomId,
        [FromBody] LanguageUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await _chatMembroService.AtualizarIdiomaPreferidoAsync(
            userId,
            UsuarioAuditoria,
            roomId,
            request,
            cancellationToken);

        return NoContent();
    }
}
