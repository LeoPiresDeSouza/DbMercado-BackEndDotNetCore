using Microsoft.AspNetCore.Identity;

namespace DbMercado.Domain.Shared.Exceptions;

/// <summary>
/// Representa falhas ao acessar ou manipular o sistema de identidade da aplicação.
/// </summary>
/// <remarks>
/// Utilizada para encapsular erros provenientes do provedor de identidade,
/// como falhas na criação de usuários, manipulação de claims ou validação de senha.
/// </remarks>
/// <example>
/// Exemplo ao utilizar serviços de identidade:
/// <code>
/// var result = await _userManager.CreateAsync(user, password);
///
/// if (!result.Succeeded)
/// {
///     throw new IdentityAccessException("Erro ao criar usuário no sistema de identidade.")
///         .With("Email", user.Email)
///         .With("IdentityErrors", result.Errors);
/// }
/// </code>
/// </example>

public class IdentityAccessException : BusinessException
{
    public IdentityAccessException(string message)
        : base("IDENTITY_ACCESS_ERROR", message)
    {
    }

    public IdentityAccessException(string message, Exception innerException)
        : base("IDENTITY_ACCESS_ERROR", message, innerException)
    {
    }
}
