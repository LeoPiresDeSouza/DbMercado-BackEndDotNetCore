using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMercado.Domain.Administracao.Interfaces.Repositories.Identity;

public interface IUsuarioIdentityRepository
{
    Task<List<IdentityUser>> GetAllAsync();
    Task<IdentityUser> GetByNameAsync(string name);
    Task<IdentityUser> GetByIdAsync(string id);

    Task DeleteAsync(string id);

    Task<List<long>> GetPermissoesAsync(IdentityUser user);
    Task<bool> HasPermissaoAsync(IdentityUser user, long permissaoId);
    Task AddPermissaoAsync(string userName, long permissaoId);
    Task RemovePermissaoAsync(string userName, long permissaoId);
    Task SetPermissoesAsync(string userName, List<long> permissoes);
}
