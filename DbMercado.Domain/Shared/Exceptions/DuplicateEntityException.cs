namespace DbMercado.Domain.Shared.Exceptions;


/// <summary>
/// Exceção utilizada quando uma operação viola uma restrição de unicidade da aplicação.
/// </summary>
/// <remarks>
/// Geralmente ocorre durante operações de criação ou atualização de entidades
/// que possuem campos únicos (ex.: e-mail, CPF, código de produto).
/// </remarks>
/// <example>
/// Exemplo de uso em um serviço de aplicação:
/// <code>
/// var usuarioExistente = await _usuarioRepository.GetByEmailAsync(email);
///
/// if (usuarioExistente != null)
/// {
///     throw new DuplicateEntityException("USER_EMAIL_ALREADY_EXISTS",
///         $"Já existe um usuário cadastrado com o e-mail '{email}'.")
///         .With("Email", email);
/// }
/// </code>
/// </example>

public class DuplicateEntityException : BusinessException
{
    public DuplicateEntityException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
