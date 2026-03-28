using DbMercado.Application.Administracao.Dtos.Modulo;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Application.Administracao.Mappers.Modulo; // Adicionado
using DbMercado.Domain.Administracao.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Administracao;

[Route("api/[controller]")]
[ApiController]
public class ModuloController : ControllerBase
{
    #region Membros privados

    private readonly IModuloService _moduloService;

    #endregion Membros privados




    #region Propriedades públicas
    #endregion Propriedades públicas
    #region ctor
    public ModuloController(IModuloService moduloService)
    {
        _moduloService = moduloService;
    }

    #endregion ctor




    #region endpoints

    /// <summary>
    /// Retorna um json com toda a estrutura de acesso do usuários aos módulos, funcionalidades
    /// e as respectivas permissões. O json é utilizado para montar o menu de acesso do usuário no frontend.
    /// </summary>
    /// <param name="request">Dto mapeado com o nome do usuário cujos módulos serão retornados</param>
    /// <returns>LoginResponse</returns>
    [Authorize]
    [HttpGet("modulosUsuario")]
    public async Task<ActionResult<List<ModuloUsuarioResponse>>> GetModulosUsuario([FromQuery] ModuloUsuarioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var modulosUsuario = await _moduloService.ModulosUsuarioAsync(request.Usuario);
        if (modulosUsuario == null) return NotFound();

        var response = modulosUsuario;
        return Ok(response);
    }

    #endregion endpoints
}
