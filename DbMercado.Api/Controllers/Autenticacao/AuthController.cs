using DbMercado.Application.Administracao.Dtos.Autenticacao;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.CrossCutting.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DbMercado.Api.Controllers.Autenticacao;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    /// <summary>
    /// Autentica o usuário no identity e retorna o token jwt com refresh token.
    /// </summary>
    /// <param name="request">Dto mapeado por LoginRequest com o usuário e senha</param>
    /// <returns>LoginResponse</returns>
    [HttpPost("login")]
    [AllowAnonymous] // Permite acesso sem token para fazer login
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var ipAddress = GetIpAddress();
        var response = await authService.AuthenticateAsync(request.Email, request.Password, ipAddress);

        if (response == null)
        {
            return Unauthorized(new { message = "Não foi possível autenticar o usuário." });
        }

        SetTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    /// <summary>
    /// Renova o token de acesso usando o refresh token.
    /// </summary>
    /// <param name="request">Refresh token request (pode vir do cookie ou do body)</param>
    /// <returns>LoginResponse com novos tokens</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        // Tenta primeiro pegar o refresh token do cookie, senão pega do body
        var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken;
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token é obrigatório." });
        }

        var ipAddress = GetIpAddress();
        var response = await authService.RefreshTokenAsync(refreshToken, ipAddress);

        if (response == null)
        {
            return Unauthorized(new { message = "Token inválido ou expirado." });
        }

        SetTokenCookie(response.RefreshToken);
        return Ok(response);
    }

    /// <summary>
    /// Revoga o refresh token (logout).
    /// </summary>
    /// <returns>Mensagem de sucesso</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Tenta pegar o refresh token do cookie ou do header
        var refreshToken = Request.Cookies["refreshToken"] ?? 
                          Request.Headers["X-Refresh-Token"].FirstOrDefault();

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token não encontrado." });
        }

        var ipAddress = GetIpAddress();
        var success = await authService.RevokeTokenAsync(refreshToken, ipAddress);

        if (!success)
        {
            return BadRequest(new { message = "Erro ao fazer logout." });
        }

        // Remove o cookie
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logout realizado com sucesso." });
    }

    /// <summary>
    /// Revoga o refresh token sem autorização (para logout forçado).
    /// </summary>
    /// <param name="request">Request com o refresh token</param>
    /// <returns>Mensagem de sucesso</returns>
    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request?.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token é obrigatório." });
        }

        var ipAddress = GetIpAddress();
        var success = await authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

        if (!success)
        {
            return BadRequest(new { message = "Token não encontrado ou já revogado." });
        }

        return Ok(new { message = "Token revogado com sucesso." });
    }

    #region Métodos Privados

    /// <summary>
    /// Obtém o endereço IP do cliente.
    /// </summary>
    private string GetIpAddress()
    {
        // Verifica se está por trás de um proxy/load balancer
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim() ?? "0.0.0.0";
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
    }

    /// <summary>
    /// Define o refresh token como cookie HTTP-only.
    /// </summary>
    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            SameSite = SameSiteMode.Strict,
            Secure = true // Usar apenas em HTTPS em produção
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    #endregion
}
