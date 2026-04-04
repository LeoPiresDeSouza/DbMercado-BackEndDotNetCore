using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Application.Produto.Services;

public sealed class MidiaService : IMidiaService
{
    private readonly IUwProduto _uw;
    private readonly IMidiaArquivoStorage _arquivos;

    public MidiaService(IUwProduto uw, IMidiaArquivoStorage arquivos)
    {
        _uw = uw;
        _arquivos = arquivos;
    }

    /// <inheritdoc />
    public async Task<MidiaUploadResponseDto> UploadTemporarioAsync(
        Stream conteudo,
        string nomeOriginal,
        string contentType,
        decimal? duracaoSegundos,
        string usuarioAuditoria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conteudo);
        if (string.IsNullOrWhiteSpace(usuarioAuditoria))
            usuarioAuditoria = ApplicationSettings.Application.AnonymousUser;

        var ct = (contentType ?? string.Empty).Trim().ToLowerInvariant();
        string tipo;
        if (ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            tipo = "imagem";
        else if (ct.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            tipo = "video";
        else
            throw new BusinessException("MIDIA_CONTENT_TYPE_INVALIDO", "Envie um arquivo image/* ou video/*.");

        var ext = ObterExtensaoSegura(nomeOriginal, tipo);
        var (urlRelativa, _) = await _arquivos.SalvarUploadAsync(conteudo, ext, cancellationToken);

        var duracao = tipo == "video" ? duracaoSegundos : null;
        var entidade = MidiaEntity.CriarTemporaria(urlRelativa, thumbnailUrl: null, tipo, duracao, usuarioAuditoria);
        await _uw.Midias.AddAsync(usuarioAuditoria, entidade);
        await _uw.SaveChangesAsync(cancellationToken);

        return new MidiaUploadResponseDto
        {
            MidiaId = entidade.Id,
            Url = entidade.Url,
            ThumbnailUrl = entidade.ThumbnailUrl,
        };
    }

    /// <inheritdoc />
    public async Task AssociarAoProdutoAsync(
        long produtoId,
        MidiaAssociarDto dto,
        string usuarioAuditoria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Itens.Count == 0)
            throw new BusinessException("MIDIA_LISTA_VAZIA", "Informe ao menos uma mídia para associar.");

        var produto = await _uw.ProdutoRepository.GetByIdAsync(produtoId);
        if (produto is null)
            throw new BusinessException("PRODUTO_NAO_ENCONTRADO", "Produto não encontrado.");

        var porId = new Dictionary<long, MidiaEntity>();
        foreach (var item in dto.Itens)
        {
            if (porId.ContainsKey(item.MidiaId))
                throw new BusinessException("MIDIA_DUPLICADA", "A mesma mídia foi informada mais de uma vez.");
            var midia = await _uw.Midias.GetByIdAsync(item.MidiaId);
            if (midia is null)
                throw new BusinessException("MIDIA_NAO_ENCONTRADA", $"Mídia {item.MidiaId} não encontrada.");
            porId[item.MidiaId] = midia;
        }

        var principaisImagens = dto.Itens.Count(i =>
            i.IsPrincipal && string.Equals(porId[i.MidiaId].Tipo, "imagem", StringComparison.Ordinal));
        if (principaisImagens > 1)
            throw new BusinessException("MIDIA_PRINCIPAL_UNICA", "Apenas uma imagem pode ser principal.");

        for (var ordem = 0; ordem < dto.Itens.Count; ordem++)
        {
            var item = dto.Itens[ordem];
            var midia = porId[item.MidiaId];

            if (midia.Status == "temporario")
            {
                if (midia.ProdutoId is not null)
                    throw new BusinessException("MIDIA_ESTADO_INVALIDO", "Mídia temporária em estado inconsistente.");
            }
            else if (midia.Status == "ativo")
            {
                if (midia.ProdutoId is not null && midia.ProdutoId != produtoId)
                    throw new BusinessException("MIDIA_PRODUTO_DIVERGENTE", "Mídia associada a outro produto.");
            }
            else
                throw new BusinessException("MIDIA_STATUS_INVALIDO", "Status de mídia não suportado.");

            midia.AssociarOuAtualizarNoProduto(produtoId, ordem, item.IsPrincipal, usuarioAuditoria);
        }

        await _uw.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MidiaResponseDto>> ListarPorProdutoAsync(
        long produtoId,
        CancellationToken cancellationToken = default)
    {
        var lista = await _uw.Midias.ListarPorProdutoIdOrdenadasAsync(produtoId, cancellationToken);
        return lista.Select(static m => new MidiaResponseDto
        {
            Id = m.Id,
            Url = m.Url,
            ThumbnailUrl = m.ThumbnailUrl,
            Tipo = m.Tipo,
            Ordem = m.Ordem,
            IsPrincipal = m.IsPrincipal,
            Duracao = m.Duracao,
            Status = m.Status,
        }).ToList();
    }

    /// <inheritdoc />
    public async Task ExcluirAsync(long midiaId, string usuarioAuditoria, CancellationToken cancellationToken = default)
    {
        var midia = await _uw.Midias.GetByIdAsync(midiaId);
        if (midia is null)
            return;

        _arquivos.ExcluirPorUrlRelativa(midia.Url);
        if (!string.IsNullOrWhiteSpace(midia.ThumbnailUrl) &&
            !string.Equals(midia.ThumbnailUrl, midia.Url, StringComparison.OrdinalIgnoreCase))
            _arquivos.ExcluirPorUrlRelativa(midia.ThumbnailUrl!);

        await _uw.Midias.DeleteAsync(midia);
        await _uw.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ExcluirTemporariasExpiradasAsync(
        int minimoHorasSemAssociacao = 24,
        CancellationToken cancellationToken = default)
    {
        var horas = Math.Clamp(minimoHorasSemAssociacao, 1, 168);
        var limite = DateTime.UtcNow.AddHours(-horas);
        var expiradas = await _uw.Midias.ListarTemporariasExpiradasAsync(limite, cancellationToken);
        foreach (var m in expiradas)
        {
            _arquivos.ExcluirPorUrlRelativa(m.Url);
            if (!string.IsNullOrWhiteSpace(m.ThumbnailUrl) &&
                !string.Equals(m.ThumbnailUrl, m.Url, StringComparison.OrdinalIgnoreCase))
                _arquivos.ExcluirPorUrlRelativa(m.ThumbnailUrl!);
            await _uw.Midias.DeleteAsync(m);
        }

        await _uw.SaveChangesAsync(cancellationToken);
    }

    private static string ObterExtensaoSegura(string nomeOriginal, string tipo)
    {
        var fallback = tipo == "video" ? ".mp4" : ".jpg";
        var ext = Path.GetExtension(nomeOriginal);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 8)
            return fallback;

        ext = ext.ToLowerInvariant();
        var permitidasImagem = new HashSet<string>(StringComparer.Ordinal) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var permitidasVideo = new HashSet<string>(StringComparer.Ordinal) { ".mp4", ".mov", ".webm", ".mkv" };
        if (tipo == "imagem" && permitidasImagem.Contains(ext))
            return ext;
        if (tipo == "video" && permitidasVideo.Contains(ext))
            return ext;

        return fallback;
    }
}
