# Implementação — Categorização de Produto

## Contexto

Adicionar um sistema de categorias hierárquicas ao cadastro de produto, com até 4 níveis
(ex.: Alimentos → Bebidas → Cervejas → Lager). A categoria é opcional na transição mas
deverá se tornar obrigatória futuramente.

A estrutura usa uma **árvore auto-referenciada** — uma única tabela onde cada nó aponta
para seu pai. Isso suporta qualquer profundidade sem alterar o schema.

---

## 1. Nomenclatura de tabelas (padrão do projeto)

O projeto usa prefixos por contexto:
- `db` → Administração/parâmetros
- `prd` → Produto

A nova tabela seguirá: **`prdCategoria`**

---

## 2. Domain — `CategoriaProdutoEntity`

**Arquivo a criar:**
`DbMercado.Domain/Produto/Entities/CategoriaProdutoEntity.cs`

```csharp
using DbMercado.Domain.Shared.Entities;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.Entities;

/// <summary>
/// Nó da árvore de categorias de produto (auto-referenciada, até 4 níveis).
/// Exemplo: Alimentos (1) → Bebidas (2) → Cervejas (3) → Lager (4).
/// </summary>
public class CategoriaProdutoEntity : BaseEntity
{
    public long Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador único para URLs e filtros (ex.: "bebidas-alcoolicas").
    /// Gerado automaticamente a partir do nome; pode ser sobrescrito.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    /// <summary>Nulo indica categoria raiz (nível 1).</summary>
    public long? CategoriaPaiId { get; private set; }

    public CategoriaProdutoEntity? CategoriaPai { get; private set; }

    public ICollection<CategoriaProdutoEntity> Subcategorias { get; private set; }
        = new List<CategoriaProdutoEntity>();

    /// <summary>Nível na hierarquia: 1 (raiz) a 4 (folha).</summary>
    public int Nivel { get; private set; }

    public bool Ativo { get; private set; } = true;

    /// <summary>Construtor para materialização pelo ORM.</summary>
    private CategoriaProdutoEntity() { }

    /// <summary>
    /// Cria uma categoria raiz (sem pai, nível 1).
    /// </summary>
    public static CategoriaProdutoEntity CriarRaiz(
        string nome,
        string? descricao,
        string usuarioAuditoria)
    {
        ValidarNome(nome);

        var agora = DateTime.UtcNow;
        return new CategoriaProdutoEntity
        {
            Nome              = nome.Trim(),
            Slug              = GerarSlug(nome),
            Descricao         = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            CategoriaPaiId    = null,
            Nivel             = 1,
            Ativo             = true,
            DataCriacao       = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao    = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    /// <summary>
    /// Cria uma subcategoria filha de outra categoria.
    /// </summary>
    public static CategoriaProdutoEntity CriarFilha(
        CategoriaProdutoEntity pai,
        string nome,
        string? descricao,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(pai);
        ValidarNome(nome);

        var nivelFilha = pai.Nivel + 1;
        if (nivelFilha > 4)
            throw new BusinessException(
                "CATEGORIA_NIVEL_MAXIMO",
                "A hierarquia de categorias suporta no máximo 4 níveis.");

        var agora = DateTime.UtcNow;
        return new CategoriaProdutoEntity
        {
            Nome              = nome.Trim(),
            Slug              = GerarSlug(nome),
            Descricao         = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            CategoriaPaiId    = pai.Id,
            CategoriaPai      = pai,
            Nivel             = nivelFilha,
            Ativo             = true,
            DataCriacao       = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao    = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };
    }

    public void Atualizar(string nome, string? descricao, string usuarioAuditoria)
    {
        ValidarNome(nome);
        Nome        = nome.Trim();
        Slug        = GerarSlug(nome);
        Descricao   = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        DataUltimaAlteracao    = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    public void Ativar(string usuarioAuditoria)
    {
        Ativo = true;
        DataUltimaAlteracao    = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    public void Inativar(string usuarioAuditoria)
    {
        Ativo = false;
        DataUltimaAlteracao    = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessException(
                "CATEGORIA_NOME_OBRIGATORIO",
                "Nome da categoria é obrigatório.");

        if (nome.Trim().Length > 128)
            throw new BusinessException(
                "CATEGORIA_NOME_MUITO_LONGO",
                "Nome da categoria deve ter no máximo 128 caracteres.");
    }

    /// <summary>
    /// Gera um slug a partir do nome: minúsculas, sem acentos, hífens no lugar de espaços.
    /// Exemplo: "Bebidas Alcoólicas" → "bebidas-alcoolicas"
    /// </summary>
    public static string GerarSlug(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return string.Empty;

        var normalizado = nome.Trim().ToLowerInvariant();

        // Remover acentos via normalização Unicode
        var semAcento = new System.Text.StringBuilder();
        foreach (var ch in normalizado.Normalize(System.Text.NormalizationForm.FormD))
        {
            var categoria = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (categoria != System.Globalization.UnicodeCategory.NonSpacingMark)
                semAcento.Append(ch);
        }

        var slug = semAcento.ToString()
            .Replace(' ', '-')
            .Replace("--", "-");

        // Manter apenas letras, dígitos e hífens
        var resultado = new System.Text.StringBuilder();
        foreach (var ch in slug)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-')
                resultado.Append(ch);
        }

        return resultado.ToString().Trim('-');
    }
}
```

---

## 3. Domain — Atualizar `ProdutoEntity`

**Arquivo:** `DbMercado.Domain/Produto/Entities/ProdutoEntity.cs`

### 3.1 Adicionar propriedades

Adicionar junto às demais propriedades (após `Gtin`, por exemplo):

```csharp
/// <summary>Categoria do produto na árvore de categorias (opcional na transição).</summary>
public long? CategoriaProdutoId { get; private set; }

public CategoriaProdutoEntity? CategoriaProduto { get; private set; }
```

### 3.2 Adicionar parâmetro no método `Registrar`

Adicionar o parâmetro (pode ser o último antes de `usuarioAuditoria`):

```csharp
long? categoriaProdutoId,
```

E no corpo:

```csharp
CategoriaProdutoId = categoriaProdutoId,
```

### 3.3 Adicionar método de atualização de categoria

```csharp
public void AlterarCategoria(long? categoriaId, string usuarioAuditoria)
{
    CategoriaProdutoId = categoriaId;
    RegistrarAuditoriaAlteracao(usuarioAuditoria);
}
```

---

## 4. Domain — Interfaces

### 4.1 `ICategoriaProdutoRepository`

**Arquivo a criar:**
`DbMercado.Domain/Produto/Interfaces/Repositories/ICategoriaProdutoRepository.cs`

```csharp
using DbMercado.Domain.Produto.Entities;

namespace DbMercado.Domain.Produto.Interfaces.Repositories;

public interface ICategoriaProdutoRepository
{
    Task<IReadOnlyList<CategoriaProdutoEntity>> ListarArvoreCompletaAsync(CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaProdutoEntity>> ListarPorNivelAsync(int nivel, CancellationToken ct = default);

    Task<CategoriaProdutoEntity?> ObterPorIdAsync(long id, CancellationToken ct = default);

    Task<CategoriaProdutoEntity?> ObterPorSlugAsync(string slug, CancellationToken ct = default);

    Task<bool> ExisteFilhaAsync(long categoriaId, CancellationToken ct = default);

    Task<bool> ExisteProdutoVinculadoAsync(long categoriaId, CancellationToken ct = default);

    void Adicionar(CategoriaProdutoEntity categoria);

    void Remover(CategoriaProdutoEntity categoria);
}
```

### 4.2 Atualizar `IUwProduto`

**Arquivo:** `DbMercado.Domain/Produto/Interfaces/UnitsOfWork/IUwProduto.cs`

Adicionar a propriedade:

```csharp
ICategoriaProdutoRepository Categorias { get; }
```

---

## 5. Application — DTOs

### 5.1 `CategoriaTreeNodeDto`

**Arquivo a criar:**
`DbMercado.Application/Produto/Dtos/CategoriaTreeNodeDto.cs`

```csharp
namespace DbMercado.Application.Produto.Dtos;

/// <summary>Nó da árvore de categorias para exibição em selects e painéis de filtro.</summary>
public class CategoriaTreeNodeDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public long? CategoriaPaiId { get; set; }
    public int Nivel { get; set; }
    public bool Ativo { get; set; }
    public IReadOnlyList<CategoriaTreeNodeDto> Subcategorias { get; set; }
        = Array.Empty<CategoriaTreeNodeDto>();
}
```

### 5.2 `CategoriaCreateDto`

**Arquivo a criar:**
`DbMercado.Application/Produto/Dtos/CategoriaCreateDto.cs`

```csharp
namespace DbMercado.Application.Produto.Dtos;

public class CategoriaCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    /// <summary>Nulo para criar categoria raiz.</summary>
    public long? CategoriaPaiId { get; set; }
}
```

### 5.3 `CategoriaUpdateDto`

**Arquivo a criar:**
`DbMercado.Application/Produto/Dtos/CategoriaUpdateDto.cs`

```csharp
namespace DbMercado.Application.Produto.Dtos;

public class CategoriaUpdateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
```

### 5.4 Atualizar `ProdutoCreateDto` e `ProdutoUpdateDto`

Adicionar em ambos:

```csharp
/// <summary>Categoria do produto (opcional).</summary>
public long? CategoriaProdutoId { get; set; }
```

### 5.5 Atualizar `ProdutoResponseDto`

Adicionar:

```csharp
public long? CategoriaProdutoId { get; set; }
public string? CategoriaNome { get; set; }
public string? CategoriaSlug { get; set; }
/// <summary>Caminho completo: ["Alimentos", "Bebidas", "Cervejas", "Lager"]</summary>
public IReadOnlyList<string> CategoriaCaminho { get; set; } = Array.Empty<string>();
```

---

## 6. Application — Interface e Service

### 6.1 `ICategoriaProdutoService`

**Arquivo a criar:**
`DbMercado.Application/Produto/Interfaces/ICategoriaProdutoService.cs`

```csharp
using DbMercado.Application.Produto.Dtos;

namespace DbMercado.Application.Produto.Interfaces;

public interface ICategoriaProdutoService
{
    Task<IReadOnlyList<CategoriaTreeNodeDto>> ListarArvoreAsync(CancellationToken ct = default);

    Task<CategoriaTreeNodeDto> CriarAsync(CategoriaCreateDto dto, string usuarioAuditoria, CancellationToken ct = default);

    Task<CategoriaTreeNodeDto> AtualizarAsync(long id, CategoriaUpdateDto dto, string usuarioAuditoria, CancellationToken ct = default);

    Task InativarAsync(long id, string usuarioAuditoria, CancellationToken ct = default);
}
```

### 6.2 `CategoriaProdutoService`

**Arquivo a criar:**
`DbMercado.Application/Produto/Services/CategoriaProdutoService.cs`

```csharp
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
        await _uw.CommitAsync(ct);

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
        await _uw.CommitAsync(ct);

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
        await _uw.CommitAsync(ct);
    }

    // ── Mapeamento ───────────────────────────────────────────────────────────

    private static CategoriaTreeNodeDto MapearArvore(
        CategoriaProdutoEntity no,
        IReadOnlyList<CategoriaProdutoEntity> todas)
    {
        var dto = MapearNo(no);
        var filhas = todas.Where(c => c.CategoriaPaiId == no.Id).ToList();
        dto = dto with
        {
            Subcategorias = filhas.Select(f => MapearArvore(f, todas)).ToList()
        };
        return dto;
    }

    private static CategoriaTreeNodeDto MapearNo(CategoriaProdutoEntity no) =>
        new()
        {
            Id             = no.Id,
            Nome           = no.Nome,
            Slug           = no.Slug,
            Descricao      = no.Descricao,
            CategoriaPaiId = no.CategoriaPaiId,
            Nivel          = no.Nivel,
            Ativo          = no.Ativo,
            Subcategorias  = Array.Empty<CategoriaTreeNodeDto>()
        };
}
```

> **Nota:** `dto with { ... }` requer que `CategoriaTreeNodeDto` seja um `record`.
> Se preferir manter como `class`, substituir pelo padrão de atribuição direta.

---

## 7. Infrastructure — EntityConfiguration

**Arquivo a criar:**
`DbMercado.Infrastructure/Produto/Persistence/Mappings/CategoriaProdutoEntityConfiguration.cs`

```csharp
using DbMercado.Domain.Produto.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Produto.Persistence.Mappings;

public class CategoriaProdutoEntityConfiguration : IEntityTypeConfiguration<CategoriaProdutoEntity>
{
    public void Configure(EntityTypeBuilder<CategoriaProdutoEntity> builder)
    {
        builder.ToTable("prdCategoria");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(128);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(160);
        builder.Property(c => c.Descricao).HasMaxLength(1000);
        builder.Property(c => c.Nivel).IsRequired();
        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.CategoriaPaiId).IsRequired(false);

        // Auto-referência: pai → filhas
        builder.HasOne(c => c.CategoriaPai)
            .WithMany(c => c.Subcategorias)
            .HasForeignKey(c => c.CategoriaPaiId)
            .OnDelete(DeleteBehavior.Restrict); // não propagar exclusão

        // Índices
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.Nivel);
        builder.HasIndex(c => c.CategoriaPaiId);
        builder.HasIndex(c => new { c.CategoriaPaiId, c.Ativo });
    }
}
```

### 7.1 Atualizar `ProdutoEntityConfiguration`

Adicionar no método `Configure` de `ProdutoEntityConfiguration`:

```csharp
// Categoria (opcional na transição)
builder.Property(p => p.CategoriaProdutoId).IsRequired(false);

builder.HasOne(p => p.CategoriaProduto)
    .WithMany()
    .HasForeignKey(p => p.CategoriaProdutoId)
    .OnDelete(DeleteBehavior.SetNull)
    .IsRequired(false);
```

---

## 8. Infrastructure — Repository e UnitOfWork

### 8.1 `CategoriaProdutoRepository`

**Arquivo a criar:**
`DbMercado.Infrastructure/Produto/Repositories/CategoriaProdutoRepository.cs`

```csharp
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.Repositories;
using DbMercado.Infrastructure.Shared.Repositories;
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
```

### 8.2 Atualizar `UwProduto`

**Arquivo:** `DbMercado.Infrastructure/Produto/UnitsOfWork/UwProduto.cs`

Adicionar a propriedade e inicialização:

```csharp
public ICategoriaProdutoRepository Categorias { get; }
```

No construtor, inicializar junto com os demais repositórios:

```csharp
Categorias = new CategoriaProdutoRepository(ctx);
```

---

## 9. Infrastructure — `AppDbContext`

**Arquivo:** `DbMercado.Infrastructure/Shared/Data/AppDbContext.cs`

Adicionar o `DbSet`:

```csharp
public DbSet<CategoriaProdutoEntity> CategoriasProduto { get; set; } = null!;
```

---

## 10. Api — Controller

**Arquivo a criar:**
`DbMercado.Api/Controllers/Produto/CategoriaProdutoController.cs`

```csharp
using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DbMercado.Api.Controllers.Produto;

[ApiController]
[Route("api/categorias-produto")]
[Authorize]
public class CategoriaProdutoController : ControllerBase
{
    private readonly ICategoriaProdutoService _service;

    public CategoriaProdutoController(ICategoriaProdutoService service)
    {
        _service = service;
    }

    /// <summary>Retorna a árvore completa de categorias.</summary>
    [HttpGet]
    public async Task<IActionResult> ListarArvore(CancellationToken ct)
    {
        var arvore = await _service.ListarArvoreAsync(ct);
        return Ok(arvore);
    }

    /// <summary>Cria uma categoria raiz ou filha.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CategoriaCreateDto dto, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var resultado = await _service.CriarAsync(dto, usuario, ct);
        return CreatedAtAction(nameof(ListarArvore), new { }, resultado);
    }

    /// <summary>Atualiza nome e descrição de uma categoria.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Atualizar(long id, [FromBody] CategoriaUpdateDto dto, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        var resultado = await _service.AtualizarAsync(id, dto, usuario, ct);
        return Ok(resultado);
    }

    /// <summary>Inativa uma categoria (sem filhas e sem produtos vinculados).</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Inativar(long id, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "sistema";
        await _service.InativarAsync(id, usuario, ct);
        return NoContent();
    }
}
```

---

## 11. DI — Registrar novos serviços

**Arquivo:** `DbMercado.Api/Program.cs` (ou no arquivo de extensão de DI do projeto)

Adicionar o registro:

```csharp
services.AddScoped<ICategoriaProdutoService, CategoriaProdutoService>();
```

O `ICategoriaProdutoRepository` é resolvido internamente pelo `UwProduto`, que já é registrado.

---

## 12. `DbInitializer` — Seed de categorias e vinculação aos produtos demo

**Arquivo:** `DbMercado.Infrastructure/Shared/Data/DbInitializer.cs`

### 12.1 Adicionar chamada no método `Initialize`

Adicionar após `AddParametros` e antes de `AddProdutosDemonstracao`:

```csharp
var mapaCategoria = AddCategorias(context, dataCarga, usuarioCarga);
```

E atualizar `AddProdutosDemonstracao` para receber o mapa:

```csharp
AddProdutosDemonstracao(context, usuarioCarga, _logger, mapaCategoria);
```

### 12.2 Novo método `AddCategorias`

```csharp
/// <summary>
/// Seed incremental de categorias. Retorna dicionário slug → Id para uso no seed de produtos.
/// </summary>
private static Dictionary<string, long> AddCategorias(
    AppDbContext context,
    DateTime dataCarga,
    string usuarioCarga)
{
    var mapa = new Dictionary<string, long>();

    // ── Nível 1 — Raízes ─────────────────────────────────────────────────
    var alimentos       = EnsureCategoria(context, "Alimentos",       null,                  1, dataCarga, usuarioCarga);
    var higieneBeleza   = EnsureCategoria(context, "Higiene e Beleza", null,                  1, dataCarga, usuarioCarga);
    var eletronicos     = EnsureCategoria(context, "Eletrônicos",      null,                  1, dataCarga, usuarioCarga);
    var utilidades      = EnsureCategoria(context, "Utilidades",       null,                  1, dataCarga, usuarioCarga);

    context.SaveChanges(); // persistir raízes para obter IDs

    // ── Nível 2 ──────────────────────────────────────────────────────────
    var graos        = EnsureCategoria(context, "Grãos e Cereais",   alimentos.Id,      2, dataCarga, usuarioCarga);
    var oleos        = EnsureCategoria(context, "Óleos e Condimentos",alimentos.Id,     2, dataCarga, usuarioCarga);
    var bebidas      = EnsureCategoria(context, "Bebidas",           alimentos.Id,      2, dataCarga, usuarioCarga);
    var cafe         = EnsureCategoria(context, "Café e Derivados",  alimentos.Id,      2, dataCarga, usuarioCarga);
    var limpeza      = EnsureCategoria(context, "Limpeza",           utilidades.Id,     2, dataCarga, usuarioCarga);
    var cuidadoPessoal = EnsureCategoria(context, "Cuidado Pessoal", higieneBeleza.Id,  2, dataCarga, usuarioCarga);
    var informatica  = EnsureCategoria(context, "Informática",       eletronicos.Id,    2, dataCarga, usuarioCarga);

    context.SaveChanges();

    // ── Nível 3 ──────────────────────────────────────────────────────────
    var arroz        = EnsureCategoria(context, "Arroz",             graos.Id,          3, dataCarga, usuarioCarga);
    var azeites      = EnsureCategoria(context, "Azeites",           oleos.Id,          3, dataCarga, usuarioCarga);
    var detergentes  = EnsureCategoria(context, "Detergentes",       limpeza.Id,        3, dataCarga, usuarioCarga);
    var notebooks    = EnsureCategoria(context, "Notebooks",         informatica.Id,    3, dataCarga, usuarioCarga);
    var cafesTorrados = EnsureCategoria(context, "Cafés Torrados",   cafe.Id,           3, dataCarga, usuarioCarga);

    context.SaveChanges();

    // ── Nível 4 ──────────────────────────────────────────────────────────
    var arrozParboilizado  = EnsureCategoria(context, "Parboilizado",    arroz.Id,       4, dataCarga, usuarioCarga);
    var azeiteExtraVirgem  = EnsureCategoria(context, "Extra Virgem",    azeites.Id,     4, dataCarga, usuarioCarga);
    var detergenteMultiuso = EnsureCategoria(context, "Multiuso",        detergentes.Id, 4, dataCarga, usuarioCarga);
    var notebookUltrafino  = EnsureCategoria(context, "Ultrafinos",      notebooks.Id,   4, dataCarga, usuarioCarga);
    var cafeEmGraos        = EnsureCategoria(context, "Em Grãos",        cafesTorrados.Id, 4, dataCarga, usuarioCarga);

    context.SaveChanges();

    // Montar mapa slug → Id para uso no seed de produtos
    foreach (var cat in context.CategoriasProduto.ToList())
        mapa[cat.Slug] = cat.Id;

    return mapa;
}

private static CategoriaProdutoEntity EnsureCategoria(
    AppDbContext context,
    string nome,
    long? paiId,
    int nivel,
    DateTime dataCarga,
    string usuarioCarga)
{
    var slug = CategoriaProdutoEntity.GerarSlug(nome);
    var existente = context.CategoriasProduto.FirstOrDefault(c => c.Slug == slug);
    if (existente is not null)
        return existente;

    var nova = new CategoriaProdutoEntity(); // EF vai materializar via construtor privado
    // Como o construtor é privado, usar os métodos de fábrica:
    CategoriaProdutoEntity entidade;
    if (paiId.HasValue)
    {
        var pai = context.CategoriasProduto.First(c => c.Id == paiId.Value);
        entidade = CategoriaProdutoEntity.CriarFilha(pai, nome, null, usuarioCarga);
    }
    else
    {
        entidade = CategoriaProdutoEntity.CriarRaiz(nome, null, usuarioCarga);
    }

    // Sobrescrever datas com dataCarga do seed
    entidade.GetType().GetProperty("DataCriacao")!.SetValue(entidade, dataCarga);
    entidade.GetType().GetProperty("DataUltimaAlteracao")!.SetValue(entidade, dataCarga);

    context.CategoriasProduto.Add(entidade);
    return entidade;
}
```

> **Nota sobre reflection:** Como `DataCriacao` e `DataUltimaAlteracao` têm setter público
> herdado de `BaseEntity`, não é necessário reflection — usar atribuição direta:
>
> ```csharp
> entidade.DataCriacao = dataCarga;
> entidade.DataUltimaAlteracao = dataCarga;
> ```

### 12.3 Vinculação dos produtos demo às categorias

Na assinatura de `AddProdutosDemonstracao`, adicionar:

```csharp
private static void AddProdutosDemonstracao(
    AppDbContext context,
    string usuarioCarga,
    ILogger logger,
    Dictionary<string, long> mapaCategoria)
```

E nas chamadas a `ProdutoEntity.Registrar`, passar o `categoriaProdutoId` correspondente:

| Produto seed | Slug da categoria | categoriaProdutoId |
|---|---|---|
| Arroz Tio João 1 kg | `parboilizado` | `mapaCategoria["parboilizado"]` |
| Azeite Andorinha 500 ml | `extra-virgem` | `mapaCategoria["extra-virgem"]` |
| Notebook 14" fictício | `ultrafinos` | `mapaCategoria["ultrafinos"]` |
| Detergente líquido 500 ml | `multiuso` | `mapaCategoria["multiuso"]` |
| Café torrado em grãos 250 g | `em-graos` | `mapaCategoria["em-graos"]` |
| Produtos extras de paginação | `graos-e-cereais` | `mapaCategoria["graos-e-cereais"]` |

Usar `mapaCategoria.GetValueOrDefault("slug")` para evitar exceção se o seed rodar
em ambiente sem categorias ainda persistidas.

---

## 13. Migration

Após todas as alterações de código, gerar a migration:

```bash
dotnet ef migrations add AddCategoriaProduto \
  --project DbMercado.Infrastructure \
  --startup-project DbMercado.Api
```

Aplicar:

```bash
dotnet ef database update \
  --project DbMercado.Infrastructure \
  --startup-project DbMercado.Api
```

---

## 14. Resumo de arquivos

| Arquivo | Ação |
|---|---|
| `DbMercado.Domain/Produto/Entities/CategoriaProdutoEntity.cs` | **Criar** |
| `DbMercado.Domain/Produto/Entities/ProdutoEntity.cs` | Adicionar `CategoriaProdutoId`, `CategoriaProduto`, `AlterarCategoria`, parâmetro em `Registrar` |
| `DbMercado.Domain/Produto/Interfaces/Repositories/ICategoriaProdutoRepository.cs` | **Criar** |
| `DbMercado.Domain/Produto/Interfaces/UnitsOfWork/IUwProduto.cs` | Adicionar `Categorias` |
| `DbMercado.Application/Produto/Dtos/CategoriaTreeNodeDto.cs` | **Criar** |
| `DbMercado.Application/Produto/Dtos/CategoriaCreateDto.cs` | **Criar** |
| `DbMercado.Application/Produto/Dtos/CategoriaUpdateDto.cs` | **Criar** |
| `DbMercado.Application/Produto/Dtos/ProdutoCreateDto.cs` | Adicionar `CategoriaProdutoId` |
| `DbMercado.Application/Produto/Dtos/ProdutoUpdateDto.cs` | Adicionar `CategoriaProdutoId` |
| `DbMercado.Application/Produto/Dtos/ProdutoResponseDto.cs` | Adicionar campos de categoria |
| `DbMercado.Application/Produto/Interfaces/ICategoriaProdutoService.cs` | **Criar** |
| `DbMercado.Application/Produto/Services/CategoriaProdutoService.cs` | **Criar** |
| `DbMercado.Infrastructure/Produto/Persistence/Mappings/CategoriaProdutoEntityConfiguration.cs` | **Criar** |
| `DbMercado.Infrastructure/Produto/Persistence/Mappings/ProdutoEntityConfiguration.cs` | Adicionar FK de categoria |
| `DbMercado.Infrastructure/Produto/Repositories/CategoriaProdutoRepository.cs` | **Criar** |
| `DbMercado.Infrastructure/Produto/UnitsOfWork/UwProduto.cs` | Adicionar `Categorias` |
| `DbMercado.Infrastructure/Shared/Data/AppDbContext.cs` | Adicionar `DbSet<CategoriaProdutoEntity>` |
| `DbMercado.Infrastructure/Shared/Data/DbInitializer.cs` | Adicionar `AddCategorias`, atualizar seed de produtos |
| `DbMercado.Api/Controllers/Produto/CategoriaProdutoController.cs` | **Criar** |
| `DbMercado.Api/Program.cs` | Registrar `ICategoriaProdutoService` |
| **Nova migration EF** | Gerar e aplicar |
