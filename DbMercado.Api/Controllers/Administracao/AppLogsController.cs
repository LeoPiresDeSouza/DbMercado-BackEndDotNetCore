using DbMercado.Application.Administracao;
using DbMercado.Application.Administracao.Dtos.AppLog;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Administracao;

[ApiController]
[Route("api/app-logs")]
[Authorize]
public class AppLogsController : ControllerBase
{
    private readonly IAppLogService _appLogService;

    public AppLogsController(IAppLogService appLogService)
    {
        _appLogService = appLogService;
    }

    [HttpPost("consultas/grid")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        AppLogPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        AppLogPermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<AppLogGridResultDto>> ConsultarGrid(
        [FromBody] AppLogGridQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _appLogService.ConsultarGridAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        AppLogPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        AppLogPermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<AppLogDetalheDto>> ObterDetalhe(long id, CancellationToken cancellationToken)
    {
        var dto = await _appLogService.ObterDetalheAsync(id, cancellationToken);
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }

    [HttpDelete("{id:long}")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        AppLogPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        AppLogPermissoesCatalogo.ExcluirEntrada
    })]
    public async Task<IActionResult> ExcluirEntrada(long id, CancellationToken cancellationToken)
    {
        var removed = await _appLogService.ExcluirEntradaAsync(id, cancellationToken);
        if (!removed)
            return NotFound();
        return NoContent();
    }

    [HttpPost("limpeza")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        AppLogPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        AppLogPermissoesCatalogo.LimparLog
    })]
    public async Task<ActionResult<LogLimpezaResultDto>> Limpeza(CancellationToken cancellationToken)
    {
        var result = await _appLogService.ExecutarLimpezaAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("backups")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        AppLogPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        AppLogPermissoesCatalogo.DescarregarParaDisco
    })]
    public async Task<ActionResult<IReadOnlyList<LogBackupArquivoDto>>> ListarBackups(CancellationToken cancellationToken)
    {
        var list = await _appLogService.ListarBackupsAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("backups/{nomeArquivo}")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        AppLogPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        AppLogPermissoesCatalogo.DescarregarParaDisco
    })]
    public async Task<IActionResult> ObterBackup(string nomeArquivo, CancellationToken cancellationToken)
    {
        var result = await _appLogService.ObterConteudoBackupAsync(nomeArquivo, cancellationToken);
        if (result is null)
            return NotFound();
        return Content(result.Value.Conteudo, "text/plain", System.Text.Encoding.UTF8);
    }
}
