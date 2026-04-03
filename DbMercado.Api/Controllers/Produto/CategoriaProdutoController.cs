using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Produto;

[ApiController]
[Route("api/categorias-produto")]
[Authorize]
public class CategoriaProdutoController : ControllerBase
{
    private readonly ICategoriaProdutoService _service;

    public CategoriaProdutoController(ICategoriaProdutoService service)
    {
        _service = service;
    }

    /// <summary>Retorna a árvore completa de categorias.</summary>
    [HttpGet]
    public async Task<IActionResult> ListarArvore(CancellationToken ct)
    {
        var arvore = await _service.ListarArvoreAsync(ct);
        return Ok(arvore);
    }

    /// <summary>Cria uma categoria raiz ou filha.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaCreateDto dto, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var resultado = await _service.CriarAsync(dto, usuario, ct);
        return CreatedAtAction(nameof(ListarArvore), new { }, resultado);
    }

    /// <summary>Atualiza nome e descrição de uma categoria.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Atualizar(long id, [FromBody] CategoriaUpdateDto dto, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var resultado = await _service.AtualizarAsync(id, dto, usuario, ct);
        return Ok(resultado);
    }

    /// <summary>Inativa uma categoria (sem filhas e sem produtos vinculados).</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Inativar(long id, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        await _service.InativarAsync(id, usuario, ct);
        return NoContent();
    }
}
