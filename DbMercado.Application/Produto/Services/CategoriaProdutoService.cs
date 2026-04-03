using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.UnitsOfWork;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Application.Produto.Services;

public class CategoriaProdutoService : ICategoriaProdutoService
{
    private readonly IUwProduto _uw;

    public CategoriaProdutoService(IUwProduto uw)
    {
        _uw = uw;
    }

    public async Task<IReadOnlyList<CategoriaTreeNodeDto>> ListarArvoreAsync(CancellationToken ct = default)
    {
        var todas = await _uw.Categorias.ListarArvoreCompletaAsync(ct);
        var raizes = todas.Where(c => c.CategoriaPaiId == null).ToList();
        return raizes.Select(r => MapearArvore(r, todas)).ToList();
    }

    public async Task<CategoriaTreeNodeDto> CriarAsync(
        CategoriaCreateDto dto,
        string usuarioAuditoria,
        CancellationToken ct = default)
    {
        CategoriaProdutoEntity categoria;

        if (dto.CategoriaPaiId.HasValue)
        {
            var pai = await _uw.Categorias.ObterPorIdAsync(dto.CategoriaPaiId.Value, ct)
                ?? throw new BusinessException("CATEGORIA_PAI_NAO_ENCONTRADA", "Categoria pai não encontrada.");

            categoria = CategoriaProdutoEntity.CriarFilha(pai, dto.Nome, dto.Descricao, usuarioAuditoria);
        }
        else
        {
            categoria = CategoriaProdutoEntity.CriarRaiz(dto.Nome, dto.Descricao, usuarioAuditoria);
        }

        _uw.Categorias.Adicionar(categoria);
        await _uw.SaveChangesAsync(ct);

        return MapearNo(categoria);
    }

    public async Task<CategoriaTreeNodeDto> AtualizarAsync(
        long id,
        CategoriaUpdateDto dto,
        string usuarioAuditoria,
        CancellationToken ct = default)
    {
        var categoria = await _uw.Categorias.ObterPorIdAsync(id, ct)
            ?? throw new BusinessException("CATEGORIA_NAO_ENCONTRADA", "Categoria não encontrada.");

        categoria.Atualizar(dto.Nome, dto.Descricao, usuarioAuditoria);
        await _uw.SaveChangesAsync(ct);

        return MapearNo(categoria);
    }

    public async Task InativarAsync(long id, string usuarioAuditoria, CancellationToken ct = default)
    {
        var categoria = await _uw.Categorias.ObterPorIdAsync(id, ct)
            ?? throw new BusinessException("CATEGORIA_NAO_ENCONTRADA", "Categoria não encontrada.");

        if (await _uw.Categorias.ExisteFilhaAsync(id, ct))
            throw new BusinessException(
                "CATEGORIA_COM_SUBCATEGORIAS",
                "Não é possível inativar uma categoria que possui subcategorias ativas.");

        if (await _uw.Categorias.ExisteProdutoVinculadoAsync(id, ct))
            throw new BusinessException(
                "CATEGORIA_COM_PRODUTOS",
                "Não é possível inativar uma categoria com produtos vinculados.");

        categoria.Inativar(usuarioAuditoria);
        await _uw.SaveChangesAsync(ct);
    }

    private static CategoriaTreeNodeDto MapearArvore(
        CategoriaProdutoEntity no,
        IReadOnlyList<CategoriaProdutoEntity> todas)
    {
        var dto = MapearNo(no);
        var filhas = todas.Where(c => c.CategoriaPaiId == no.Id).ToList();
        dto.Subcategorias = filhas.Select(f => MapearArvore(f, todas)).ToList();
        return dto;
    }

    private static CategoriaTreeNodeDto MapearNo(CategoriaProdutoEntity no) =>
        new()
        {
            Id = no.Id,
            Nome = no.Nome,
            Slug = no.Slug,
            Descricao = no.Descricao,
            CategoriaPaiId = no.CategoriaPaiId,
            Nivel = no.Nivel,
            Ativo = no.Ativo,
            Subcategorias = Array.Empty<CategoriaTreeNodeDto>()
        };
}
