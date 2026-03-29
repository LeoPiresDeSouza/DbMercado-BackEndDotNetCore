using System.Security.Claims;
using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using DbMercado.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Produto;

[Authorize]
[Route("api/produtos")]
[ApiController]
public class ProdutoController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutoController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    private string UsuarioAuditoria =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.Email)
        ?? ApplicationSettings.Application.AnonymousUser;

    [HttpPost]
    public async Task<ActionResult<long>> Criar(
        [FromBody] ProdutoCreateDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _produtoService.CriarProdutoAsync(UsuarioAuditoria, request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id }, id);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Atualizar(
        long id,
        [FromBody] ProdutoUpdateDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _produtoService.AtualizarProdutoAsync(UsuarioAuditoria, id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancellationToken)
    {
        await _produtoService.ExcluirProdutoAsync(UsuarioAuditoria, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProdutoResponseDto>> ObterPorId(long id, CancellationToken cancellationToken)
    {
        var produto = await _produtoService.ObterPorIdAsync(id, cancellationToken);
        return produto is null ? NotFound() : Ok(produto);
    }

    [HttpGet("{id:long}/logistica")]
    public async Task<ActionResult<ProdutoLogisticaResponseDto>> ObterLogistica(long id, CancellationToken cancellationToken)
    {
        var logistica = await _produtoService.ObterLogisticaAsync(id, cancellationToken);
        return logistica is null ? NotFound() : Ok(logistica);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProdutoResumoDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await _produtoService.ListarAsync(cancellationToken);
        return Ok(lista);
    }

    [HttpGet("consultas/por-ncm")]
    public async Task<ActionResult<IReadOnlyList<ProdutoListItemDto>>> BuscarPorNcm(
        [FromQuery] string ncm,
        CancellationToken cancellationToken)
    {
        var lista = await _produtoService.BuscarPorNcmAsync(ncm, cancellationToken);
        return Ok(lista);
    }

    [HttpGet("consultas/por-origem")]
    public async Task<ActionResult<IReadOnlyList<ProdutoListItemDto>>> BuscarPorOrigem(
        [FromQuery] string tipo,
        CancellationToken cancellationToken)
    {
        var lista = await _produtoService.BuscarPorOrigemGeograficaAsync(tipo, cancellationToken);
        return Ok(lista);
    }

    [HttpGet("consultas/por-unidade-medida")]
    public async Task<ActionResult<IReadOnlyList<ProdutoListItemDto>>> BuscarPorUnidadeMedida(
        [FromQuery] string unidade,
        CancellationToken cancellationToken)
    {
        var lista = await _produtoService.BuscarPorUnidadeMedidaAsync(unidade, cancellationToken);
        return Ok(lista);
    }

    [HttpGet("consultas/por-marca")]
    public async Task<ActionResult<IReadOnlyList<ProdutoListItemDto>>> BuscarPorMarca(
        [FromQuery] string marca,
        CancellationToken cancellationToken)
    {
        var lista = await _produtoService.BuscarPorMarcaAsync(marca, cancellationToken);
        return Ok(lista);
    }
}
