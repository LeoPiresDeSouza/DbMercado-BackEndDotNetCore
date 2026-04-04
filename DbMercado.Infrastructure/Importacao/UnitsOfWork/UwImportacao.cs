using DbMercado.Domain.Importacao.Interfaces.Repositories;
using DbMercado.Domain.Importacao.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Importacao.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;
using DbMercado.Infrastructure.Shared.UnitsOfWork;

namespace DbMercado.Infrastructure.Importacao.UnitsOfWork;

public class UwImportacao : IUwImportacao
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;
    private readonly IApplicationCachingFactory _cacheFactory;

    private INotaFiscalRepository? _notaFiscalRepository;
    private IProdutoImportadoRepository? _produtoImportadoRepository;

    public UwImportacao(
        AppDbContext context,
        IRepositoryFactory repoFactory,
        IApplicationCachingFactory cacheFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
        _cacheFactory = cacheFactory;
    }

    public INotaFiscalRepository NotaFiscalRepository =>
        _notaFiscalRepository ??= _repoFactory.Create<NotaFiscalRepository>(_context);

    public IProdutoImportadoRepository ProdutoImportadoRepository =>
        _produtoImportadoRepository ??= _repoFactory.Create<ProdutoImportadoRepository>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var linhas = await _context.SaveChangesAsync(cancellationToken);
        UnitOfWorkCacheInvalidacao.AposSaveSeAlterou(
            _cacheFactory,
            linhas,
            UnitOfWorkCacheInvalidacao.Importacao);
        return linhas;
    }
}
