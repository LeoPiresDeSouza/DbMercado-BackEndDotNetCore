using DbMercado.Application.Produto.Dtos;

namespace DbMercado.Application.Produto.Interfaces;

public interface IMidiaService
{
    Task<MidiaUploadResponseDto> UploadTemporarioAsync(
        Stream conteudo,
        string nomeOriginal,
        string contentType,
        decimal? duracaoSegundos,
        string usuarioAuditoria,
        CancellationToken cancellationToken = default);

    Task AssociarAoProdutoAsync(
        long produtoId,
        MidiaAssociarDto dto,
        string usuarioAuditoria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MidiaResponseDto>> ListarPorProdutoAsync(
        long produtoId,
        CancellationToken cancellationToken = default);

    Task ExcluirAsync(long midiaId, string usuarioAuditoria, CancellationToken cancellationToken = default);

    /// <param name="minimoHorasSemAssociacao">Remove temporárias com <c>DataCriacao</c> mais antiga que este limite (UTC).</param>
    Task ExcluirTemporariasExpiradasAsync(
        int minimoHorasSemAssociacao = 24,
        CancellationToken cancellationToken = default);
}
