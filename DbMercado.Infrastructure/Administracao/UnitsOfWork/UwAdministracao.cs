using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Domain.Administracao.Interfaces.Repositories.Identity;
using DbMercado.Domain.Administracao.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Administracao.Repositories.Administracao;
using DbMercado.Infrastructure.Administracao.Repositories.Identity;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;

namespace DbMercado.Infrastructure.Administracao.UnitsOfWork;

public class UwAdministracao : IUwAdministracao
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;
    private readonly IApplicationCachingFactory _cachingFactory;

    private IModuloRepository? _moduloRepository;
    private IUsuarioIdentityRepository? _usuarioIdentityRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;

    public UwAdministracao(
        AppDbContext context,
        IRepositoryFactory repoFactory,
        IApplicationCachingFactory cachingFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
        _cachingFactory = cachingFactory;
    }

    public IModuloRepository ModuloRepository =>
        _moduloRepository ??= _repoFactory.Create<ModuloRepository>(_context);

    public IUsuarioIdentityRepository UsuarioIdentityRepository =>
        _usuarioIdentityRepository ??= _repoFactory.Create<UsuarioIdentityRepository>();

    /// <summary>
    /// Fora do <see cref="IRepositoryFactory"/> para evitar falhas do <c>ActivatorUtilities</c> com o cache genérico.
    /// </summary>
    public IRefreshTokenRepository RefreshTokenRepository =>
        _refreshTokenRepository ??= new RefreshTokenRepository(_context, _cachingFactory.GetApplicationCaching<RefreshTokenEntity>());

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
