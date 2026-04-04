using DbMercado.Application.Produto.Dtos;

namespace DbMercado.Application.Produto.Interfaces;

public interface IProdutoService
{
    Task<long> CriarProdutoAsync(string usuarioAutenticado, ProdutoCreateDto dto, CancellationToken cancellationToken = default);

    Task AtualizarProdutoAsync(string usuarioAutenticado, long id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default);

    Task ExcluirProdutoAsync(string usuarioAutenticado, long id, CancellationToken cancellationToken = default);

    Task<ProdutoResponseDto?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<ProdutoLogisticaResponseDto?> ObterLogisticaAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoResumoDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<ProdutoGridResultDto> ConsultarGridAsync(ProdutoGridQueryDto query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorNcmAsync(string ncm, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorOrigemGeograficaAsync(
        string tipoOrigemCodigo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorUnidadeMedidaAsync(
        string unidadeMedida,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorMarcaAsync(string marca, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesComercializacaoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesMedidaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarTiposEmbalagemAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesDimensaoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesPesoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarOrigensGeograficasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarOrigensIcmsAsync(CancellationToken cancellationToken = default);
}
