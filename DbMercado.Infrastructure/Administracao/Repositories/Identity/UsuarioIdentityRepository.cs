using DbMercado.Domain.Shared.Exceptions;
using DbMercado.Domain.Administracao.Interfaces.Repositories.Identity;
using DbMercado.Domain.Administracao.ValueObjects.Identity;
using Microsoft.AspNetCore.Identity;

namespace DbMercado.Infrastructure.Administracao.Repositories.Identity;

/// <summary>
/// Serviço responsável por operações de alto nível relacionadas ao usuário do Identity,
/// incluindo projeção para <see cref="UsuarioIdentity"/> e manipulação de permissões.
/// </summary>
public class UsuarioIdentityRepository : BaseIdentityRepository, IUsuarioIdentityRepository
{
    #region Membros Privados

    /// <summary>
    /// Tipo da claim de permissão.
    /// </summary>
    private const string ClaimPermissao = "Permissao";

    #endregion Membros Privados




    #region Propriedades públicas
    #endregion Propriedades públicas

    


    #region Ctor

    /// <summary>
    /// Inicializa uma nova instância do serviço.
    /// </summary>
    public UsuarioIdentityRepository(UserManager<IdentityUser> userManager)
        : base(userManager)
    {
    }

    #endregion Ctor




    #region Métodos públicos de seleção

    /// <summary>
    /// Recupera todos os usuários do Identity projetados para <see cref="UsuarioIdentity"/>.
    /// </summary>
    public async Task<List<IdentityUser>> GetAllAsync()
    {
        var usuarios = await GetAllIdentityUsersAsync();

        if (!usuarios.Any())
            return new List<IdentityUser>();

        return usuarios;
    }



    /// <summary>
    /// Recupera um usuário pelo nome.
    /// </summary>
    public async Task<IdentityUser> GetByNameAsync(string name)
    {
        var user = await FindIdentityUserByNameAsync(name)
            ?? throw new IdentityAccessException("Não foi possível encontrar o usuário.");

        return user;
    }



    /// <summary>
    /// Recupera um usuário pelo id.
    /// </summary>
    public async Task<IdentityUser> GetByIdAsync(string id)
    {
        var user = await FindIdentityUserAsync(id)
            ?? throw new IdentityAccessException("Não foi possível encontrar o usuário.");

        return user;
    }

    #endregion




    #region Métodos de manipulação de usuários

    /// <summary>
    /// Remove um usuário do Identity.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        var user = await FindIdentityUserAsync(id)
            ?? throw new IdentityAccessException("Não foi possível encontrar o usuário.");

        await DeleteUser(user);
    }

    #endregion




    #region Métodos de permissões

    /// <summary>
    /// Retorna todas as permissões do usuário.
    /// </summary>
    public async Task<List<long>> GetPermissoesAsync(IdentityUser user)
    {
        try
        {
            var claims = await GetClaimsAsync(user);

            return claims
                .Where(c => c.Type == ClaimPermissao)
                .Select(c => long.TryParse(c.Value, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new IdentityAccessException("Erro ao recuperar permissões do usuário.", ex);
        }
    }



    /// <summary>
    /// Verifica se o usuário possui uma permissão específica.
    /// </summary>
    public async Task<bool> HasPermissaoAsync(IdentityUser user, long permissaoId)
    {
        try
        {
            var claims = await GetClaimsAsync(user);

            return claims.Any(c =>
                c.Type == ClaimPermissao &&
                c.Value == permissaoId.ToString());
        }
        catch (Exception ex)
        {
            throw new IdentityAccessException("Erro ao verificar permissão do usuário.", ex);
        }
    }



    /// <summary>
    /// Adiciona uma permissão ao usuário.
    /// </summary>
    public async Task AddPermissaoAsync(string userName, long permissaoId)
    {
        var user = await FindIdentityUserByNameAsync(userName)
            ?? throw new IdentityAccessException("Usuário não encontrado.");

        try
        {
            var hasPermissao = await HasPermissaoAsync(user, permissaoId);

            if (hasPermissao)
                return;

            await AddClaimAsync(ClaimPermissao, permissaoId.ToString(), user);
        }
        catch (Exception ex)
        {
            throw new IdentityAccessException("Erro ao adicionar permissão ao usuário.", ex);
        }
    }



    /// <summary>
    /// Remove uma permissão específica do usuário.
    /// </summary>
    public async Task RemovePermissaoAsync(string userName, long permissaoId)
    {
        var user = await FindIdentityUserByNameAsync(userName)
            ?? throw new IdentityAccessException("Usuário não encontrado.");

        try
        {
            var claims = await GetClaimsAsync(user);

            var claimsToRemove = claims
                .Where(c => c.Type == ClaimPermissao && c.Value == permissaoId.ToString())
                .ToList();

            foreach (var claim in claimsToRemove)
            {
                await RemoveClaimAsync(claim.Type, user);
            }
        }
        catch (Exception ex)
        {
            throw new IdentityAccessException("Erro ao remover permissão do usuário.", ex);
        }
    }



    /// <summary>
    /// Substitui todas as permissões do usuário pelas informadas.
    /// </summary>
    public async Task SetPermissoesAsync(string userName, List<long> permissoes)
    {
        var user = await FindIdentityUserByNameAsync(userName)
            ?? throw new IdentityAccessException("Usuário não encontrado.");

        try
        {
            await RemoveClaimAsync(ClaimPermissao, user);

            var claims = permissoes
                .Distinct()
                .Select(p => new IdentityUserClaims
                {
                    ClaimType = ClaimPermissao,
                    ClaimValue = p.ToString()
                })
                .ToList();

            await AddClaimsAsync(claims, user);
        }
        catch (Exception ex)
        {
            throw new IdentityAccessException("Erro ao definir permissões do usuário.", ex);
        }
    }

    #endregion

}
