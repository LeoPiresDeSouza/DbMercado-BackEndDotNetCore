using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Domain.Administracao.Interfaces.Repositories.Identity;
using DbMercado.Domain.Administracao.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Administracao.Repositories.Administracao;
using DbMercado.Infrastructure.Administracao.Repositories.Identity;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.UnitsOfWork;

namespace DbMercado.Infrastructure.Administracao.UnitsOfWork;

public class UwAdministracao : IUwAdministracao
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;
    private readonly IApplicationCachingFactory _cacheFactory;

    private IModuloRepository? _moduloRepository;
    private IUsuarioIdentityRepository? _usuarioIdentityRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;

    public UwAdministracao(
        AppDbContext context,
        IRepositoryFactory repoFactory,
        IApplicationCachingFactory cacheFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
        _cacheFactory = cacheFactory;
    }

    public IModuloRepository ModuloRepository =>
        _moduloRepository ??= _repoFactory.Create<ModuloRepository>(_context);

    public IUsuarioIdentityRepository UsuarioIdentityRepository =>
        _usuarioIdentityRepository ??= _repoFactory.Create<UsuarioIdentityRepository>();

    /// <summary>
    /// Fora do <see cref="IRepositoryFactory"/> para evitar falhas do <c>ActivatorUtilities</c> com o cache genérico.
    /// </summary>
    public IRefreshTokenRepository RefreshTokenRepository =>
        _refreshTokenRepository ??= new RefreshTokenRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        var linhas = await _context.SaveChangesAsync();
        UnitOfWorkCacheInvalidacao.AposSaveSeAlterou(
            _cacheFactory,
            linhas,
            UnitOfWorkCacheInvalidacao.Administracao);
        return linhas;
    }
}
