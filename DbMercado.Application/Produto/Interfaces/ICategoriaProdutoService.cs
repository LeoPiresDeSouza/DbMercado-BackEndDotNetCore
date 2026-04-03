using DbMercado.Application.Produto.Dtos;

namespace DbMercado.Application.Produto.Interfaces;

public interface ICategoriaProdutoService
{
    Task<IReadOnlyList<CategoriaTreeNodeDto>> ListarArvoreAsync(CancellationToken ct = default);

    Task<CategoriaTreeNodeDto> CriarAsync(CategoriaCreateDto dto, string usuarioAuditoria, CancellationToken ct = default);

    Task<CategoriaTreeNodeDto> AtualizarAsync(long id, CategoriaUpdateDto dto, string usuarioAuditoria, CancellationToken ct = default);

    Task InativarAsync(long id, string usuarioAuditoria, CancellationToken ct = default);
}
