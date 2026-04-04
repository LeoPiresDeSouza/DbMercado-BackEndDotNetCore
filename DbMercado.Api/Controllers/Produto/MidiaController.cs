using DbMercado.Api.Extensions;
using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Produto;

[Authorize]
[Route("api/produtos/midia")]
[ApiController]
public class MidiaController : ControllerBase
{
    private readonly IMidiaService _midiaService;

    public MidiaController(IMidiaService midiaService)
    {
        _midiaService = midiaService;
    }

    private string UsuarioAuditoria => User.ResolveUsuarioAuditoria();

    /// <summary>Upload local; persiste como temporário até associação ao produto.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(524_288_000)]
    public async Task<ActionResult<MidiaUploadResponseDto>> Upload(
        IFormFile arquivo,
        [FromForm] decimal? duracao,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest("Arquivo obrigatório.");

        await using var stream = arquivo.OpenReadStream();
        var dto = await _midiaService.UploadTemporarioAsync(
            stream,
            arquivo.FileName,
            arquivo.ContentType ?? string.Empty,
            duracao,
            UsuarioAuditoria,
            cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{midiaId:long}")]
    public async Task<IActionResult> Excluir(long midiaId, CancellationToken cancellationToken)
    {
        await _midiaService.ExcluirAsync(midiaId, UsuarioAuditoria, cancellationToken);
        return NoContent();
    }
}
