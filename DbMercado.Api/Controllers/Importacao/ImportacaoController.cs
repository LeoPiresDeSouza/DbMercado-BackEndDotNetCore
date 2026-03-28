using System.Security.Claims;
using DbMercado.Application.Importacao.Dtos;
using DbMercado.Application.Importacao.Interfaces;
using DbMercado.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Importacao;

[Authorize]
[Route("api/importacao")]
[ApiController]
public class ImportacaoController : ControllerBase
{
    private readonly IImportacaoService _importacaoService;

    public ImportacaoController(IImportacaoService importacaoService)
    {
        _importacaoService = importacaoService;
    }

    private string UsuarioAuditoria =>
        User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.Email)
        ?? ApplicationSettings.Application.AnonymousUser;

    [HttpPost("notas-fiscais")]
    public async Task<ActionResult<long>> CadastrarNotaFiscal(
        [FromBody] NotaFiscalCadastroRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _importacaoService.CadastrarNotaFiscalAsync(UsuarioAuditoria, request, cancellationToken);
        return CreatedAtAction(nameof(ObterNotaFiscal), new { id }, id);
    }

    [HttpGet("notas-fiscais/{id:long}")]
    public async Task<ActionResult<NotaFiscalResponse>> ObterNotaFiscal(long id, CancellationToken cancellationToken)
    {
        var nf = await _importacaoService.ObterNotaFiscalAsync(id, cancellationToken);
        return nf is null ? NotFound() : Ok(nf);
    }

    [HttpGet("notas-fiscais")]
    public async Task<ActionResult<IReadOnlyList<NotaFiscalResumoResponse>>> ListarNotasFiscais(CancellationToken cancellationToken)
    {
        var lista = await _importacaoService.ListarNotasFiscaisAsync(cancellationToken);
        return Ok(lista);
    }

    [HttpPost("produtos-importados")]
    public async Task<ActionResult<long>> CadastrarProdutoImportado(
        [FromBody] ProdutoImportadoCadastroRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _importacaoService.CadastrarProdutoImportadoAsync(UsuarioAuditoria, request, cancellationToken);
        return CreatedAtAction(nameof(ObterProdutoImportado), new { id }, id);
    }

    [HttpGet("produtos-importados/{id:long}")]
    public async Task<ActionResult<ProdutoImportadoResponse>> ObterProdutoImportado(long id, CancellationToken cancellationToken)
    {
        var p = await _importacaoService.ObterProdutoImportadoAsync(id, cancellationToken);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpGet("produtos-importados")]
    public async Task<ActionResult<IReadOnlyList<ProdutoImportadoResumoResponse>>> ListarProdutosImportados(CancellationToken cancellationToken)
    {
        var lista = await _importacaoService.ListarProdutosImportadosAsync(cancellationToken);
        return Ok(lista);
    }
}
