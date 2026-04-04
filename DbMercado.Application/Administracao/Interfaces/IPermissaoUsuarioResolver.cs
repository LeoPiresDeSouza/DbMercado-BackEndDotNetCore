using System.Security.Claims;

namespace DbMercado.Application.Administracao.Interfaces;

/// <summary>
/// Verifica se o usuário autenticado possui a permissão (claim <c>Permissao</c> com id da tabela <c>dbPermissao</c>).
/// </summary>
public interface IPermissaoUsuarioResolver
{
    Task<bool> UsuarioPossuiPermissaoAsync(
        ClaimsPrincipal usuario,
        string funcionalidadeNomeNormalizado,
        string permissaoNome,
        CancellationToken cancellationToken = default);
}
