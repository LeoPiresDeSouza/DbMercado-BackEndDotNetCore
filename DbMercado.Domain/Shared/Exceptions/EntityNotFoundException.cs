namespace DbMercado.Domain.Shared.Exceptions;


/// <summary>
/// Exceção lançada quando uma entidade solicitada não pode ser encontrada.
/// </summary>
/// <remarks>
/// Geralmente utilizada em operações de consulta, atualização ou exclusão
/// quando o identificador informado não corresponde a nenhum registro existente.
/// </remarks>
/// <example>
/// Exemplo de uso em um serviço de aplicação:
/// <code>
/// var produto = await _produtoRepository.GetByIdAsync(id);
///
/// if (produto == null)
/// {
///     throw new EntityNotFoundException("Produto", id);
/// }
/// </code>
/// </example>

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
    }

    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} com ID '{id}' não foi encontrado.")
    {
    }
}
