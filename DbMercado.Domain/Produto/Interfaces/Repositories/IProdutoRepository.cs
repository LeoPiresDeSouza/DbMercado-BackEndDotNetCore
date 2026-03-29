using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Queries;
using DbMercado.Domain.Shared.Interfaces.Repositories;

namespace DbMercado.Domain.Produto.Interfaces.Repositories;

public interface IProdutoRepository : IBaseRepository<ProdutoEntity>
{
    /// <summary>
    /// Carrega o agregado com SKUs, value objects owned e atributos.
    /// </summary>
    /// <param name="rastrear">Quando verdadeiro, a entidade fica rastreada para alteração e persistência via SaveChanges no contexto.</param>
    Task<ProdutoEntity?> GetByIdCompletoAsync(long id, bool rastrear = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista produtos para catálogo (sem rastreamento, ordenado por nome).
    /// </summary>
    Task<List<ProdutoEntity>> ListarCatalogoAsync(CancellationToken cancellationToken = default);

    /// <summary>NCM já normalizado (8 dígitos).</summary>
    Task<List<ProdutoEntity>> BuscarPorNcmAsync(string ncm, CancellationToken cancellationToken = default);

    /// <summary><paramref name="tipoOrigemCodigo"/> já normalizado (ex.: caixa alta).</summary>
    Task<List<ProdutoEntity>> BuscarPorOrigemGeograficaAsync(
        string tipoOrigemCodigo,
        CancellationToken cancellationToken = default);

    /// <summary>Unidade de medida já normalizada para comparação (ex.: caixa alta).</summary>
    Task<List<ProdutoEntity>> BuscarPorUnidadeMedidaAsync(string unidadeMedida, CancellationToken cancellationToken = default);

    /// <summary>Fragmento de marca (contém, sem diferenciar maiúsculas/minúsculas).</summary>
    Task<List<ProdutoEntity>> BuscarPorMarcaContendoAsync(string marca, CancellationToken cancellationToken = default);

    /// <summary>Lista resumida com total para grid (sem includes; filtros e ordenação no servidor).</summary>
    Task<(List<ProdutoEntity> Items, int TotalCount)> ConsultarGridAsync(
        ProdutoGridSpecification spec,
        CancellationToken cancellationToken = default);
}
