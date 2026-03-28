using DbMercado.Application.Importacao.Dtos;

namespace DbMercado.Application.Importacao.Interfaces;

public interface IImportacaoService
{
    Task<long> CadastrarNotaFiscalAsync(string usuarioAutenticado, NotaFiscalCadastroRequest request, CancellationToken cancellationToken = default);

    Task<NotaFiscalResponse?> ObterNotaFiscalAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotaFiscalResumoResponse>> ListarNotasFiscaisAsync(CancellationToken cancellationToken = default);

    Task<long> CadastrarProdutoImportadoAsync(string usuarioAutenticado, ProdutoImportadoCadastroRequest request, CancellationToken cancellationToken = default);

    Task<ProdutoImportadoResponse?> ObterProdutoImportadoAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoImportadoResumoResponse>> ListarProdutosImportadosAsync(CancellationToken cancellationToken = default);
}
