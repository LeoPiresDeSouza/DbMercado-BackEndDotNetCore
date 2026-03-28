using DbMercado.Domain.Importacao.Interfaces.Repositories;
using DbMercado.Domain.Importacao.Interfaces.UnitsOfWork;
using DbMercado.Infrastructure.Importacao.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using DbMercado.Infrastructure.Shared.Interfaces;

namespace DbMercado.Infrastructure.Importacao.UnitsOfWork;

public class UwImportacao : IUwImportacao
{
    private readonly AppDbContext _context;
    private readonly IRepositoryFactory _repoFactory;

    private INotaFiscalRepository? _notaFiscalRepository;
    private IProdutoImportadoRepository? _produtoImportadoRepository;

    public UwImportacao(AppDbContext context, IRepositoryFactory repoFactory)
    {
        _context = context;
        _repoFactory = repoFactory;
    }

    public INotaFiscalRepository NotaFiscalRepository =>
        _notaFiscalRepository ??= _repoFactory.Create<NotaFiscalRepository>(_context);

    public IProdutoImportadoRepository ProdutoImportadoRepository =>
        _produtoImportadoRepository ??= _repoFactory.Create<ProdutoImportadoRepository>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
