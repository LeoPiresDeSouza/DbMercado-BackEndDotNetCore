using DbMercado.Domain.Administracao.ValueObjects.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DbMercado.Domain.Shared.Interfaces.Repositories;

public interface IBaseIdentityRepository<T>
{
    Task<IdentityUser?> FindIdentityUserByNameAsync(string userName);
    Task<IdentityUser?> FindIdentityUserByEmailAsync(string email);
    Task<IdentityUser?> FindIdentityUserAsync(string identityUserId);
    Task<List<IdentityUser>> GetAllIdentityUsersAsync();
    Task<IdentityUser> AddUser(IdentityUsuarioAdd identityUsuario, string password);
    Task DeleteUser(IdentityUser user);
    Task<List<Claim>> GetClaimsAsync(IdentityUser user);
    Task<List<Claim>> GetClaimsAsync(string claimType, IdentityUser user);
    Task AddClaimAsync(string claimType, string claimValue, IdentityUser user);
    Task AddClaimsAsync(List<IdentityUserClaims> userClaims, IdentityUser user);
    Task RemoveClaimAsync(string claimType, IdentityUser user);
    Task RemoveUserClaimsAsync(IdentityUser user);
}
