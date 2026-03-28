using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Domain.Administracao.Interfaces.Repositories
{
    /// <summary>
    /// Interface do repositório de Refresh Token
    /// </summary>
    public interface IRefreshTokenRepository : IBaseRepository<RefreshTokenEntity>
    {
        /// <summary>
        /// Busca um refresh token pelo valor do token
        /// </summary>
        Task<RefreshTokenEntity> GetByTokenAsync(string token);

        /// <summary>
        /// Busca todos os refresh tokens ativos de um usuário
        /// </summary>
        Task<IEnumerable<RefreshTokenEntity>> GetActiveTokensByUserIdAsync(string userId);

        /// <summary>
        /// Revoga todos os refresh tokens de um usuário
        /// </summary>
        Task RevokeAllUserTokensAsync(string userId, string ipAddress);

        /// <summary>
        /// Revoga um refresh token específico
        /// </summary>
        Task RevokeTokenAsync(string token, string ipAddress, string? replacedByToken = null);

        /// <summary>
        /// Remove tokens expirados (limpeza)
        /// </summary>
        Task RemoveExpiredTokensAsync();

        /// <summary>
        /// Verifica se um token existe e está ativo
        /// </summary>
        Task<bool> IsTokenActiveAsync(string token);
    }
}