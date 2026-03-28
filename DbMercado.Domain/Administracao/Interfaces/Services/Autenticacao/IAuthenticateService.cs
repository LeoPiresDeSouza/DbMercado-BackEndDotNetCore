using Microsoft.AspNetCore.Identity;

namespace DbMercado.Domain.Administracao.Interfaces.Services.Autenticacao;

public interface IAuthenticateService
{
    /// <summary>
    /// Autentica um usuário no identity e devolve um token JWT.
    /// </summary>
    /// <param name="email">email do usuário no identity.</param>
    /// <param name="password">senha do usuário no identity.</param>
    /// <returns>string?</returns>
    Task<string?> AuthenticateAsync(string email, string password);
    
    /// <summary>
    /// Gera um token JWT para o usuário especificado.
    /// </summary>
    /// <param name="user">Usuário do Identity.</param>
    /// <returns>Token JWT</returns>
    Task<string> GenerateJwtTokenAsync(IdentityUser user);
}
