using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Domain.Produto.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Produto.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;

namespace DbMercado.Infrastructure.Produto.UnitsOfWork;

public class UwProduto : IUwProduto
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;

    private IProdutoRepository? _produtoRepository;

    private ICategoriaProdutoRepository? _categorias;

    public UwProduto(AppDbContext context, IRepositoryFactory repoFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
    }

    public IProdutoRepository ProdutoRepository =>
        _produtoRepository ??= _repoFactory.Create<ProdutoRepository>(_context);

    public ICategoriaProdutoRepository Categorias =>
        _categorias ??= _repoFactory.Create<CategoriaProdutoRepository>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
