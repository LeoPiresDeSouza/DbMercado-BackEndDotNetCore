using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace DbMercado.Infrastructure.Produto.Repositories;

public class CategoriaProdutoRepository : ICategoriaProdutoRepository
{
    private readonly AppDbContext _ctx;

    public CategoriaProdutoRepository(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IReadOnlyList<CategoriaProdutoEntity>> ListarArvoreCompletaAsync(CancellationToken ct = default)
        => await _ctx.CategoriasProduto
            .OrderBy(c => c.Nivel)
            .ThenBy(c => c.Nome)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CategoriaProdutoEntity>> ListarPorNivelAsync(int nivel, CancellationToken ct = default)
        => await _ctx.CategoriasProduto
            .Where(c => c.Nivel == nivel && c.Ativo)
            .OrderBy(c => c.Nome)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<CategoriaProdutoEntity?> ObterPorIdAsync(long id, CancellationToken ct = default)
        => await _ctx.CategoriasProduto.FindAsync([id], ct);

    public async Task<CategoriaProdutoEntity?> ObterPorSlugAsync(string slug, CancellationToken ct = default)
        => await _ctx.CategoriasProduto
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<bool> ExisteFilhaAsync(long categoriaId, CancellationToken ct = default)
        => await _ctx.CategoriasProduto
            .AnyAsync(c => c.CategoriaPaiId == categoriaId && c.Ativo, ct);

    public async Task<bool> ExisteProdutoVinculadoAsync(long categoriaId, CancellationToken ct = default)
        => await _ctx.Produtos
            .AnyAsync(p => p.CategoriaProdutoId == categoriaId, ct);

    public void Adicionar(CategoriaProdutoEntity categoria)
        => _ctx.CategoriasProduto.Add(categoria);

    public void Remover(CategoriaProdutoEntity categoria)
        => _ctx.CategoriasProduto.Remove(categoria);
}
