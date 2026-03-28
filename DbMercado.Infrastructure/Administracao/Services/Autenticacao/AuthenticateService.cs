using DbMercado.CrossCutting.Settings;
using DbMercado.Domain.Administracao.Interfaces.Services.Autenticacao;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DbMercado.Infrastructure.Administracao.Services.Autenticacao;

public class AuthenticateService : IAuthenticateService
{

    #region Membros privados

    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    #endregion Membros privados




    #region Propriedades públicas
    #endregion Propriedades públicas




    #region Ctor

    public AuthenticateService(UserManager<IdentityUser> userManager,
                          SignInManager<IdentityUser> signInManager,
                          IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    #endregion Ctor




    #region Métodos públicos

    /// <summary>
    /// Autentica um usuário no identity e devolve um token JWT.
    /// </summary>
    /// <param name="email">email do usuário no identity.</param>
    /// <param name="password">senha do usuário no identity.</param>
    /// <returns>string?</returns>
    public async Task<string?> AuthenticateAsync(string email, string password)
    {
        // Recupera o usuário no identity

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        // Verifica a senha

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return null;

        // Gera o Token
        return await GenerateJwtTokenAsync(user);
    }

    /// <summary>
    /// Gera um token JWT para o usuário especificado.
    /// </summary>
    /// <param name="user">Usuário do Identity.</param>
    /// <returns>Token JWT</returns>
    public async Task<string> GenerateJwtTokenAsync(IdentityUser user)
    {
        return await GenerateJwtToken(user);
    }

    #endregion Métodos públicos




    #region Métodos privados

    /// <summary>
    /// Gera um token JWT
    /// </summary>
    /// <param name="user">ApplicationUser com o mapeamento para o IdentityUser.</param>
    /// <returns>string</returns>
    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        // Busca as roles/claims do usuário no banco
        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        // Claims padrões do JWT
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.UserName!) // Nome do usuário
        };

        // Adiciona as claims personalizadas do Identity
        claims.AddRange(userClaims);

        // Adiciona as roles como Claims
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Configuração da chave de criptografia
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion Métodos privados
}
