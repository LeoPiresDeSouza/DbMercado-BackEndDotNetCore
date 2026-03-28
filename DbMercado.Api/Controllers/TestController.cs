using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public()
    {
        return Ok(new { message = "Este endpoint é público e não requer autenticação." });
    }

    [HttpGet("authenticated")]
    public IActionResult Authenticated()
    {
        var userId = User.FindFirst("userId")?.Value ?? User.FindFirst("sub")?.Value;
        var userName = User.Identity?.Name ?? User.FindFirst("name")?.Value;
        
        return Ok(new 
        { 
            message = "Este endpoint requer autenticação.",
            userId = userId,
            userName = userName,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Policy = "CanViewLogs")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Este endpoint requer role Admin." });
    }

    [HttpGet("menu-manager")]
    [Authorize(Policy = "CanManageMenus")]
    public IActionResult MenuManager()
    {
        return Ok(new { message = "Este endpoint requer permissão 'menus.manage'." });
    }

    [HttpGet("read-permission")]
    [Authorize(Policy = "CanRead")]
    public IActionResult ReadPermission()
    {
        return Ok(new { message = "Este endpoint requer permissão 'read'." });
    }
}
