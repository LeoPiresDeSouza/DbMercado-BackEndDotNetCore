using DbMercado.Domain.Shared.Exceptions;
using DbMercado.Domain.Shared.Interfaces.Repositories;
using DbMercado.Domain.Administracao.ValueObjects.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DbMercado.Infrastructure.Shared.Repositories;

/// <summary>
/// Classe base responsável por prover operações comuns de manipulação de usuários
/// e claims no ASP.NET Identity.
/// </summary>
/// <remarks>
/// Esta classe funciona como um serviço de infraestrutura reutilizável por serviços
/// especializados que manipulam usuários do Identity.
/// </remarks>
public abstract class BaseIdentityRepository
{
    #region Membros protegidos

    /// <summary>
    /// Gerenciador de usuários do ASP.NET Identity responsável por operações
    /// de criação, remoção e manipulação de claims.
    /// </summary>
    protected readonly UserManager<IdentityUser> _userManager;

    #endregion




    #region Ctor

    /// <summary>
    /// Inicializa uma nova instância do serviço base de Identity.
    /// </summary>
    /// <param name="userManager">Instância do gerenciador de usuários do Identity.</param>
    /// <exception cref="ArgumentNullException">Lançada quando o gerenciador de usuários é nulo.</exception>
    protected BaseIdentityRepository(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    #endregion Ctor




    #region Métodos de seleção de usuários Identity

    /// <summary>
    /// Recupera um usuário do Identity a partir do seu nome de usuário.
    /// </summary>
    /// <param name="userName">Nome do usuário no Identity.</param>
    /// <returns>
    /// Instância de <see cref="IdentityUser"/> caso o usuário seja encontrado;
    /// caso contrário retorna <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando o nome do usuário é inválido.</exception>
    public async Task<IdentityUser?> FindIdentityUserByNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Nome do usuário inválido.", nameof(userName));

        return await _userManager.FindByNameAsync(userName);
    }



    /// <summary>
    /// Recupera um usuário do Identity a partir do seu endereço de e-mail.
    /// </summary>
    /// <param name="email">Email do usuário no Identity.</param>
    /// <returns>
    /// Instância de <see cref="IdentityUser"/> caso o usuário seja encontrado;
    /// caso contrário retorna <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando o email é inválido.</exception>
    public async Task<IdentityUser?> FindIdentityUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email inválido.", nameof(email));

        return await _userManager.FindByEmailAsync(email);
    }



    /// <summary>
    /// Recupera um usuário do Identity a partir do seu identificador único.
    /// </summary>
    /// <param name="identityUserId">Identificador do usuário no Identity.</param>
    /// <returns>
    /// Instância de <see cref="IdentityUser"/> caso o usuário seja encontrado;
    /// caso contrário retorna <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Lançada quando o identificador informado é inválido.</exception>
    public async Task<IdentityUser?> FindIdentityUserAsync(string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new ArgumentException("Id do usuário inválido.", nameof(identityUserId));

        return await _userManager.FindByIdAsync(identityUserId);
    }



    /// <summary>
    /// Recupera todos os usuários cadastrados no Identity.
    /// </summary>
    /// <returns>Lista contendo todos os usuários do Identity.</returns>
    public async Task<List<IdentityUser>> GetAllIdentityUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    #endregion




    #region Métodos de inclusão e exclusão de usuários

    /// <summary>
    /// Cria um novo usuário no Identity.
    /// </summary>
    /// <param name="usuario">
    /// Instância da classe <see cref="UsuarioIdentity"/> contendo os dados do usuário.
    /// </param>
    /// <param name="password">Senha inicial do usuário.</param>
    /// <returns>Instância do usuário criado no Identity.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário informado é nulo.</exception>
    /// <exception cref="DbApplicationException">
    /// Lançada quando ocorre erro na criação do usuário no Identity.
    /// </exception>
    public async Task<IdentityUser> AddUser(IdentityUsuarioAdd identityUsuario, string password)
    {
        ArgumentNullException.ThrowIfNull(identityUsuario);

        var user = new IdentityUser
        {
            UserName = identityUsuario.Nome,
            Email = identityUsuario.Email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new IdentityAccessException("Erro ao criar usuário no sistema de identidade.")
                .With("Email", user.Email)
                .With("IdentityErrors", result.Errors);
        }

        return user;
    }



    /// <summary>
    /// Remove um usuário do Identity.
    /// </summary>
    /// <remarks>
    /// Antes da remoção do usuário todas as suas claims são removidas.
    /// </remarks>
    /// <param name="user">Usuário do Identity a ser removido.</param>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    /// <exception cref="DbApplicationException">Lançada quando ocorre erro ao remover o usuário.</exception>
    public async Task DeleteUser(IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        await RemoveUserClaimsAsync(user);

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new IdentityAccessException("Erro ao excluir usuário no sistema de identidade.")
                .With("Email", user.Email)
                .With("IdentityErrors", result.Errors);
        }
    }

    #endregion




    #region Métodos de manipulação de Claims

    /// <summary>
    /// Recupera todas as claims associadas a um usuário do Identity.
    /// </summary>
    /// <param name="user">Usuário do Identity.</param>
    /// <returns>Lista contendo todas as claims associadas ao usuário.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    public async Task<List<Claim>> GetClaimsAsync(IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = await _userManager.GetClaimsAsync(user);
        return claims.ToList();
    }



    /// <summary>
    /// Recupera todas as claims de um determinado tipo associadas ao usuário.
    /// </summary>
    /// <param name="claimType">Tipo da claim.</param>
    /// <param name="user">Usuário do Identity.</param>
    /// <returns>Lista contendo as claims do tipo informado.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    public async Task<List<Claim>> GetClaimsAsync(string claimType, IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == claimType)
            .ToList();
    }



    /// <summary>
    /// Adiciona uma claim ao usuário no Identity.
    /// </summary>
    /// <param name="claimType">Tipo da claim.</param>
    /// <param name="claimValue">Valor da claim.</param>
    /// <param name="user">Usuário do Identity.</param>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    /// <exception cref="DbApplicationException">Lançada quando ocorre erro ao adicionar a claim.</exception>
    public async Task AddClaimAsync(string claimType, string claimValue, IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claim = new Claim(claimType, claimValue);

        var result = await _userManager.AddClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            throw new IdentityAccessException("Erro incluind claim para o usuário no sistema de identidade.")
                .With("Email", user.Email)
                .With("IdentityErrors", result.Errors);
        }
    }



    /// <summary>
    /// Adiciona múltiplas claims ao usuário no Identity.
    /// </summary>
    /// <param name="userClaims">Coleção de claims a serem adicionadas.</param>
    /// <param name="user">Usuário do Identity.</param>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    /// <exception cref="DbApplicationException">Lançada quando ocorre erro ao adicionar as claims.</exception>
    public async Task AddClaimsAsync(List<IdentityUserClaims> userClaims, IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (userClaims == null || !userClaims.Any())
            return;

        var claims = userClaims
            .Select(c => new Claim(c.ClaimType, c.ClaimValue))
            .ToList();

        var result = await _userManager.AddClaimsAsync(user, claims);

        if (!result.Succeeded)
        {
            throw new IdentityAccessException("Erro incluind claims para o usuário no sistema de identidade.")
                .With("Email", user.Email)
                .With("IdentityErrors", result.Errors);
        }
    }



    /// <summary>
    /// Remove todas as claims de um determinado tipo associadas ao usuário.
    /// </summary>
    /// <param name="claimType">Tipo da claim a ser removida.</param>
    /// <param name="user">Usuário do Identity.</param>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    /// <exception cref="DbApplicationException">Lançada quando ocorre erro ao remover as claims.</exception>
    public async Task RemoveClaimAsync(string claimType, IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == claimType)
            .ToList();

        if (!claims.Any())
            return;

        var result = await _userManager.RemoveClaimsAsync(user, claims);

        if (!result.Succeeded)
        {
            throw new IdentityAccessException("Erro removendo claim para o usuário no sistema de identidade.")
                .With("Email", user.Email)
                .With("IdentityErrors", result.Errors);
        }
    }



    /// <summary>
    /// Remove todas as claims associadas a um usuário.
    /// </summary>
    /// <param name="user">Usuário do Identity.</param>
    /// <exception cref="ArgumentNullException">Lançada quando o usuário é nulo.</exception>
    /// <exception cref="DbApplicationException">Lançada quando ocorre erro ao remover as claims.</exception>
    public async Task RemoveUserClaimsAsync(IdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = await _userManager.GetClaimsAsync(user);

        if (!claims.Any())
            return;

        var result = await _userManager.RemoveClaimsAsync(user, claims);

        if (!result.Succeeded)
        {
            throw new IdentityAccessException("Erro removenco claims para o usuário no sistema de identidade.")
                .With("Email", user.Email)
                .With("IdentityErrors", result.Errors);
        }
    }

    #endregion
}
