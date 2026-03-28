using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Domain.Administracao.Interfaces.Repositories.Identity;

namespace DbMercado.Domain.Administracao.Interfaces.UnitsOfWork;

public interface IUwAdministracao
{
    IModuloRepository ModuloRepository { get; }

    IUsuarioIdentityRepository UsuarioIdentityRepository { get; }

    IRefreshTokenRepository RefreshTokenRepository { get; }

    Task<int> SaveChangesAsync();
}
