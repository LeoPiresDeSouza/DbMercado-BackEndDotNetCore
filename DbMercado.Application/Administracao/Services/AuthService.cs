using System.Security.Cryptography;
using DbMercado.Application.Administracao.Dtos.Autenticacao;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Shared;
using DbMercado.Domain.Administracao.Interfaces.Services.Autenticacao;
using DbMercado.Domain.Administracao.Interfaces.UnitsOfWork;
using Microsoft.AspNetCore.Identity;

namespace DbMercado.Application.Administracao.Services;

public class AuthService : IAuthService
{
    #region Membros privados

    private readonly IAuthenticateService _authRepository;
    private readonly IUwAdministracao _uwAdministracao;
    private readonly UserManager<IdentityUser> _userManager;

    #endregion Membros privados

    #region Ctor

    public AuthService(
        IAuthenticateService authRepository,
        IUwAdministracao uwAdministracao,
        UserManager<IdentityUser> userManager)
    {
        _authRepository = authRepository;
        _uwAdministracao = uwAdministracao;
        _userManager = userManager;
    }

    #endregion Ctor

    #region Métodos públicos

    /// <summary>
    /// Autentica o usuário no identity. Se autenticado, retorna o token jwt com refresh token.
    /// </summary>
    public async Task<LoginResponse?> AuthenticateAsync(string email, string password, string ipAddress)
    {
        // Autentica o usuário
        var token = await _authRepository.AuthenticateAsync(email, password);

        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        // Busca o usuário
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        // Revoga refresh tokens antigos do usuário
        await _uwAdministracao.RefreshTokenRepository.RevokeAllUserTokensAsync(user.Id, ipAddress);

        // Gera o refresh token
        var refreshToken = GenerateRefreshToken(user.Id, ipAddress);
        
        // Salva o refresh token
        var auditUser = user.UserName ?? user.Email ?? ApplicationSettings.Application.AnonymousUser;
        await _uwAdministracao.RefreshTokenRepository.AddAsync(auditUser, refreshToken);
        await _uwAdministracao.SaveChangesAsync();

        // Retorna a resposta com os tokens
        return new LoginResponse(
            token,
            DateTime.UtcNow.AddHours(1), // Token expira em 1 hora
            refreshToken.Token,
            refreshToken.ExpiryDate
        );
    }

    /// <summary>
    /// Renova o token de acesso usando o refresh token.
    /// </summary>
    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        // Busca o refresh token
        var storedToken = await _uwAdministracao.RefreshTokenRepository.GetByTokenAsync(refreshToken);
        
        if (storedToken == null || !storedToken.IsActive)
        {
            return null;
        }

        // Busca o usuário
        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null)
        {
            return null;
        }

        // Revoga o token antigo
        await _uwAdministracao.RefreshTokenRepository.RevokeTokenAsync(
            storedToken.Token, 
            ipAddress, 
            null // será substituído pelo novo token
        );

        // Gera novo refresh token
        var newRefreshToken = GenerateRefreshToken(user.Id, ipAddress);
        
        // Atualiza o token antigo com o replacement token
        await _uwAdministracao.RefreshTokenRepository.RevokeTokenAsync(
            storedToken.Token, 
            ipAddress, 
            newRefreshToken.Token
        );

        // Salva o novo refresh token
        var auditUserRefresh = user.UserName ?? user.Email ?? ApplicationSettings.Application.AnonymousUser;
        await _uwAdministracao.RefreshTokenRepository.AddAsync(auditUserRefresh, newRefreshToken);
        await _uwAdministracao.SaveChangesAsync();

        // Gera novo access token
        var newAccessToken = await _authRepository.GenerateJwtTokenAsync(user);

        return new LoginResponse(
            newAccessToken,
            DateTime.UtcNow.AddHours(1), // Token expira em 1 hora
            newRefreshToken.Token,
            newRefreshToken.ExpiryDate
        );
    }

    /// <summary>
    /// Revoga o refresh token (logout).
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _uwAdministracao.RefreshTokenRepository.GetByTokenAsync(refreshToken);
        
        if (token == null || !token.IsActive)
        {
            return false;
        }

        // Revoga o token
        await _uwAdministracao.RefreshTokenRepository.RevokeTokenAsync(
            token.Token, 
            ipAddress
        );
        
        await _uwAdministracao.SaveChangesAsync();
        
        return true;
    }

    #endregion Métodos públicos

    #region Métodos privados

    /// <summary>
    /// Gera um novo refresh token
    /// </summary>
    private RefreshTokenEntity GenerateRefreshToken(string userId, string ipAddress)
    {
        using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        var randomBytes = new byte[64];
        rngCryptoServiceProvider.GetBytes(randomBytes);

        return new RefreshTokenEntity
        {
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token válido por 7 dias
            IsRevoked = false,
            ReplacedByToken = string.Empty,
            CreatedIpAddress = ipAddress,
            RevokedIpAddress = string.Empty,
            DataCriacao = DateTime.UtcNow,
            DataUltimaAlteracao = DateTime.UtcNow
        };
    }

    #endregion Métodos privados
}
