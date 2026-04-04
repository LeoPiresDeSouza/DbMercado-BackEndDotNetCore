using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.Entities;

/// <summary>
/// Imagem ou vídeo do catálogo vinculado a um <see cref="ProdutoEntity"/>.
/// </summary>
public class MidiaEntity : BaseEntity
{
    public long Id { get; private set; }

    public long? ProdutoId { get; private set; }

    public ProdutoEntity? Produto { get; private set; }

    public string Url { get; private set; } = string.Empty;

    public string? ThumbnailUrl { get; private set; }

    /// <summary>imagem | video</summary>
    public string Tipo { get; private set; } = string.Empty;

    public int Ordem { get; private set; }

    public bool IsPrincipal { get; private set; }

    /// <summary>Duração em segundos; apenas vídeo.</summary>
    public decimal? Duracao { get; private set; }

    /// <summary>temporario | ativo</summary>
    public string Status { get; private set; } = string.Empty;

    public MidiaEntity()
    {
    }

    public static MidiaEntity CriarTemporaria(
        string url,
        string? thumbnailUrl,
        string tipo,
        decimal? duracao,
        string usuarioAuditoria)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new BusinessException("MIDIA_URL_OBRIGATORIA", "URL da mídia é obrigatória.");

        var t = tipo.Trim().ToLowerInvariant();
        if (t is not ("imagem" or "video"))
            throw new BusinessException("MIDIA_TIPO_INVALIDO", "Tipo de mídia deve ser 'imagem' ou 'video'.");

        var agora = DateTime.UtcNow;
        return new MidiaEntity
        {
            Url = url.Trim(),
            ThumbnailUrl = string.IsNullOrWhiteSpace(thumbnailUrl) ? null : thumbnailUrl.Trim(),
            Tipo = t,
            Ordem = 0,
            IsPrincipal = false,
            Duracao = duracao,
            Status = "temporario",
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria,
        };
    }

    /// <summary>
    /// Associa mídia temporária ao produto ou atualiza ordem/principal de mídia já ativa do mesmo produto.
    /// </summary>
    public void AssociarOuAtualizarNoProduto(
        long produtoId,
        int ordem,
        bool isPrincipal,
        string usuarioAuditoria)
    {
        if (ProdutoId is not null && ProdutoId != produtoId)
            throw new BusinessException("MIDIA_PRODUTO_DIVERGENTE", "A mídia pertence a outro produto.");

        var principal = isPrincipal && Tipo == "imagem";
        ProdutoId = produtoId;
        Ordem = ordem;
        IsPrincipal = principal;
        Status = "ativo";
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    public void DesmarcarPrincipal(string usuarioAuditoria)
    {
        if (!IsPrincipal)
            return;

        IsPrincipal = false;
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }
}
