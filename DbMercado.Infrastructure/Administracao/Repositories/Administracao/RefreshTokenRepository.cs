using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Administracao.Repositories.Administracao
{
    /// <summary>
    /// Repositório de Refresh Token
    /// </summary>
    public class RefreshTokenRepository : BaseRepository<RefreshTokenEntity>, IRefreshTokenRepository
    {
        /// <summary>Cache alinhado ao <see cref="RepositoryFactory"/> (mesma assinatura que <see cref="ModuloRepository"/>).</summary>
        private readonly IApplicationCachingService<RefreshTokenEntity> _cache;

        public RefreshTokenRepository(
            AppDbContext context,
            IApplicationCachingService<RefreshTokenEntity> cache) : base(context)
        {
            _cache = cache;
        }

        /// <summary>
        /// Busca um refresh token pelo valor do token
        /// </summary>
        public async Task<RefreshTokenEntity> GetByTokenAsync(string token)
        {
            return await DbSet
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Busca todos os refresh tokens ativos de um usuário
        /// </summary>
        public async Task<IEnumerable<RefreshTokenEntity>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await DbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(rt => rt.DataCriacao)
                .ToListAsync();
        }

        /// <summary>
        /// Revoga todos os refresh tokens de um usuário
        /// </summary>
        public async Task RevokeAllUserTokensAsync(string userId, string ipAddress)
        {
            var tokens = await DbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedDate = DateTime.UtcNow;
                token.RevokedIpAddress = ipAddress;
                token.DataUltimaAlteracao = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Revoga um refresh token específico
        /// </summary>
        public async Task RevokeTokenAsync(string token, string ipAddress, string? replacedByToken = null)
        {
            var refreshToken = await GetByTokenAsync(token);
            
            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedDate = DateTime.UtcNow;
                refreshToken.RevokedIpAddress = ipAddress;
                refreshToken.ReplacedByToken = replacedByToken ?? string.Empty;
                refreshToken.DataUltimaAlteracao = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Remove tokens expirados (limpeza)
        /// </summary>
        public async Task RemoveExpiredTokensAsync()
        {
            // Remove tokens que expiraram há mais de 30 dias
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            
            var expiredTokens = await DbSet
                .Where(rt => rt.ExpiryDate < cutoffDate)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                DbSet.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Verifica se um token existe e está ativo
        /// </summary>
        public async Task<bool> IsTokenActiveAsync(string token)
        {
            var refreshToken = await GetByTokenAsync(token);
            return refreshToken != null && refreshToken.IsActive;
        }
    }
}