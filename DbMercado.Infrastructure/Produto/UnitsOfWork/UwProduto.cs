using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Domain.Produto.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Produto.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.UnitsOfWork;

namespace DbMercado.Infrastructure.Produto.UnitsOfWork;

public class UwProduto : IUwProduto
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;
    private readonly IApplicationCachingFactory _cacheFactory;

    private IProdutoRepository? _produtoRepository;

    private ICategoriaProdutoRepository? _categorias;

    private IMidiaRepository? _midias;

    public UwProduto(
        AppDbContext context,
        IRepositoryFactory repoFactory,
        IApplicationCachingFactory cacheFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
        _cacheFactory = cacheFactory;
    }

    public IProdutoRepository ProdutoRepository =>
        _produtoRepository ??= _repoFactory.Create<ProdutoRepository>(_context);

    public ICategoriaProdutoRepository Categorias =>
        _categorias ??= _repoFactory.Create<CategoriaProdutoRepository>(_context);

    public IMidiaRepository Midias =>
        _midias ??= _repoFactory.Create<MidiaRepository>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var linhas = await _context.SaveChangesAsync(cancellationToken);
        UnitOfWorkCacheInvalidacao.AposSaveSeAlterou(
            _cacheFactory,
            linhas,
            UnitOfWorkCacheInvalidacao.Produto);
        return linhas;
    }
}
