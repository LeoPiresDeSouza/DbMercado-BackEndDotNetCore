using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DbMercado.Domain.Shared;

namespace DbMercado.Api.Extensions;

/// <summary>
/// O JWT gerado pelo <c>AuthenticateService</c> inclui <see cref="JwtRegisteredClaimNames.Email"/> (claim curta <c>email</c>),
/// não necessariamente <see cref="ClaimTypes.Email"/> — sem um fallback explícito o nome de auditoria pode ficar vazio e
/// serviços que validam whitespace acabam em <see cref="ArgumentException"/> (500).
/// </summary>
public static class ClaimsPrincipalAuditoriaExtensions
{
    public static string ResolveUsuarioAuditoria(this ClaimsPrincipal? user)
    {
        static string? PrimeiroNaoVazio(params string?[] valores)
        {
            foreach (var v in valores)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
            }
            return null;
        }

        var nome = PrimeiroNaoVazio(
            user?.FindFirstValue(ClaimTypes.Name),
            user?.FindFirstValue("unique_name"),
            user?.FindFirstValue("name"),
            user?.FindFirstValue(ClaimTypes.Email),
            user?.FindFirstValue(JwtRegisteredClaimNames.Email),
            user?.FindFirstValue(ClaimTypes.NameIdentifier),
            user?.Identity?.Name);

        return string.IsNullOrWhiteSpace(nome)
            ? ApplicationSettings.Application.AnonymousUser
            : nome;
    }

    /// <summary>ID do usuário no ASP.NET Identity (<c>AspNetUsers.Id</c>), a partir do JWT.</summary>
    public static string? ResolveUserId(this ClaimsPrincipal? user)
    {
        var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        id = user?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        return user?.FindFirstValue("sub");
    }
}
