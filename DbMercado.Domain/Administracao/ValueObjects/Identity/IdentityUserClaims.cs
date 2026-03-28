namespace DbMercado.Domain.Administracao.ValueObjects.Identity;

/// <summary>
/// Mapeia as informações básicas para criação de claims de usuários no identity.
/// </summary>
/// 
public record IdentityUserClaims
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}
