using System;

namespace DbMercado.Domain.Administracao.Entities
{
    /// <summary>
    /// Entidade para gerenciar Refresh Tokens
    /// </summary>
    public class RefreshTokenEntity : BaseEntity
    {
        /// <summary>
        /// ID do usuário associado ao token
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Token de refresh
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Data de expiração do token
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Indica se o token foi revogado
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Data em que o token foi revogado
        /// </summary>
        public DateTime? RevokedDate { get; set; }

        /// <summary>
        /// Token que substituiu este (quando um novo refresh token é gerado)
        /// </summary>
        public string ReplacedByToken { get; set; }

        /// <summary>
        /// IP de criação do token
        /// </summary>
        public string CreatedIpAddress { get; set; }

        /// <summary>
        /// IP de revogação do token
        /// </summary>
        public string RevokedIpAddress { get; set; }

        /// <summary>
        /// Verifica se o token está ativo (não expirado e não revogado)
        /// </summary>
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;

        /// <summary>
        /// Verifica se o token expirou
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

        // Navegação
        public virtual UsuarioEntity Usuario { get; set; }
    }
}