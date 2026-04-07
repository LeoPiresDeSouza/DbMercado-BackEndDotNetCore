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
/// Salas de chat, convites por sala e histórico paginado de mensagens.
/// Requer JWT e permissão <c>acessar</c> na funcionalidade <c>chatmultilingue</c> (módulo Administração).
/// </summary>
[Authorize]
[ApiController]
[Route("api/chat/rooms")]
public sealed class ChatRoomsController : ControllerBase
{
    private readonly IChatSalaService _chatSalaService;
    private readonly IChatMensagemService _chatMensagemService;

    public ChatRoomsController(
        IChatSalaService chatSalaService,
        IChatMensagemService chatMensagemService)
    {
        _chatSalaService = chatSalaService;
        _chatMensagemService = chatMensagemService;
    }

    private string UsuarioAuditoria => User.ResolveUsuarioAuditoria();

    /// <summary>Cria uma nova sala; o usuário autenticado torna-se dono e membro.</summary>
    /// <param name="request">Nome, descrição opcional e demais dados da sala.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>Sala criada e cabeçalho <c>Location</c> com o recurso.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<RoomResponse>> Criar(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _chatSalaService.CriarSalaAsync(userId, UsuarioAuditoria, request, cancellationToken);
        return Created($"/api/chat/rooms/{result.Id}", result);
    }

    /// <summary>Lista salas em que o usuário é membro, com papel e idioma preferido na sala.</summary>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> ListarMinhas(CancellationToken cancellationToken)
    {
        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var lista = await _chatSalaService.ListarMinhasSalasAsync(userId, cancellationToken);
        return Ok(lista);
    }

    /// <summary>
    /// Histórico paginado de mensagens da sala, ordenado por <c>SentAt</c> descendente (mais recentes primeiro).
    /// O campo <c>content</c> de cada item segue o <c>LanguagePref</c> do membro na sala (com fallback ao texto original).
    /// </summary>
    /// <param name="id">Identificador da sala.</param>
    /// <param name="page">Número da página (≥ 1). Padrão: 1.</param>
    /// <param name="pageSize">Itens por página (1 a 100). Padrão: 50.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(ChatMessagesPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<ChatMessagesPageResponse>> ListarMensagens(
        [FromRoute] Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _chatMensagemService.ListarMensagensSalaPaginadoAsync(
            userId,
            id,
            page,
            pageSize,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Convida um usuário para a sala (apenas dono ou admin da sala).</summary>
    /// <param name="id">Identificador da sala.</param>
    /// <param name="request">Usuário convidado e metadados do convite.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpPost("{id:guid}/invite")]
    [ProducesResponseType(typeof(InviteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        ChatMultilinguePermissoesCatalogo.FuncionalidadeNomeNormalizado,
        ChatMultilinguePermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<InviteResponse>> Convidar(
        [FromRoute] Guid id,
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _chatSalaService.ConvidarUsuarioAsync(
            userId,
            UsuarioAuditoria,
            id,
            request,
            cancellationToken);
        return Ok(result);
    }
}
