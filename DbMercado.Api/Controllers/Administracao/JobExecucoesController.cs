using DbMercado.Application.Administracao;
using DbMercado.Application.Administracao.Dtos.JobExecucao;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Administracao;

[ApiController]
[Route("api/job-execucoes")]
[Authorize]
public class JobExecucoesController : ControllerBase
{
    private readonly IJobExecucaoAdministracaoService _service;

    public JobExecucoesController(IJobExecucaoAdministracaoService service)
    {
        _service = service;
    }

    [HttpPost("consultas/grid")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        JobExecucaoPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        JobExecucaoPermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<JobExecucaoGridResultDto>> ConsultarGrid(
        [FromBody] JobExecucaoGridQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.ConsultarGridAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [TypeFilter(typeof(RequirePermissaoFilter), Arguments = new object[]
    {
        JobExecucaoPermissoesCatalogo.FuncionalidadeNomeNormalizado,
        JobExecucaoPermissoesCatalogo.Acessar
    })]
    public async Task<ActionResult<JobExecucaoDetalheDto>> ObterDetalhe(long id, CancellationToken cancellationToken)
    {
        var dto = await _service.ObterDetalheAsync(id, cancellationToken);
        if (dto is null)
            return NotFound();
        return Ok(dto);
    }
}
