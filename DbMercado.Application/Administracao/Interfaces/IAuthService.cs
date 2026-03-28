using DbMercado.Application.Administracao.Dtos.Autenticacao;

namespace DbMercado.Application.Administracao.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Autentica o usuário no identity. Se autenticado, retorna o token jwt com refresh token.
    /// </summary>
    /// <param name="email">email do usuário no identity.</param>
    /// <param name="password">senha do usuário no identity.</param>
    /// <param name="ipAddress">endereço IP do cliente.</param>
    /// <returns>LoginResponse com tokens ou null</returns>
    Task<LoginResponse?> AuthenticateAsync(string email, string password, string ipAddress);
    
    /// <summary>
    /// Renova o token de acesso usando o refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token válido.</param>
    /// <param name="ipAddress">endereço IP do cliente.</param>
    /// <returns>LoginResponse com novos tokens ou null</returns>
    Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string ipAddress);
    
    /// <summary>
    /// Revoga o refresh token (logout).
    /// </summary>
    /// <param name="refreshToken">Refresh token a ser revogado.</param>
    /// <param name="ipAddress">endereço IP do cliente.</param>
    /// <returns>true se revogado com sucesso</returns>
    Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
}
