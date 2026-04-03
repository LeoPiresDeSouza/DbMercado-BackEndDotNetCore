# Refatoração — Separação de Unidades no Cadastro de Produto

## Contexto e problema

O campo `UnidadeMedida` em `ProdutoEntity` está sendo usado como um campo genérico para
finalidades diferentes — comercialização, medida física, embalagem, dimensões e peso —
misturadas em uma única lista de parâmetros (`produto / unidadeMedida`).

Isso precisa ser separado em **cinco conceitos distintos**, cada um com sua própria lista
de parâmetros e seu próprio campo na entidade.

---

## Mapa de mudanças — visão geral

| O que muda | Onde |
|---|---|
| Separar `UnidadeMedida` em 3 campos na entidade | `ProdutoEntity` |
| Adicionar unidade às dimensões (produto e embalagem) | `DimensaoProduto`, `DimensaoEmbalagem` |
| Atualizar método `Registrar` e `AtualizarDadosBasicos` | `ProdutoEntity` |
| Atualizar `Validar()` | `ProdutoEntity` |
| Renomear e ampliar constantes | `ProdutoParametrosCatalogo` |
| Substituir seed de unidades | `DbInitializer` |
| Criar nova migration | EF Core |
| Atualizar DTOs | `ProdutoCreateDto`, `ProdutoUpdateDto`, `ProdutoDimensaoDto`, `ProdutoDimensaoEmbalagemDto` |
| Atualizar mapeamento EF | `ProdutoEntityConfiguration` |

---

## 1. Conceitos a separar

| Conceito | Campo na entidade | Atributo no Parâmetro | Pergunta que responde |
|---|---|---|---|
| **Unidade de comercialização** | `UnidadeComercializacao` | `unidadeComercializacao` | Como o produto é vendido/faturado? (UN, KIT, DZ, CX...) |
| **Unidade de medida física** | `UnidadeMedidaFisica` | `unidadeMedida` | Qual a natureza física para NF-e? (UN, KG, L, M...) |
| **Tipo de embalagem** | `TipoEmbalagem` | `unidadeEmbalagem` | Como o produto é acondicionado? (CX, FD, PCT, LAT...) |
| **Unidade das dimensões** | em `DimensaoProduto` e `DimensaoEmbalagem` | `unidadeDimensao` | Altura/largura/comprimento em que unidade? (CM, M, MM) |
| **Unidade do peso** | em `DimensaoEmbalagem` (e `DimensaoProduto` se necessário) | `unidadePeso` | Peso em que unidade? (KG, G, T) |

---

## 2. `ProdutoParametrosCatalogo.cs`

**Arquivo:** `DbMercado.Application/Produto/ProdutoParametrosCatalogo.cs`

Substituir o conteúdo completo por:

```csharp
namespace DbMercado.Application.Produto;

/// <summary>
/// Convenção de <see cref="DbMercado.Domain.Administracao.Entities.ParametroEntity"/>
/// para o domínio de produto. Valores alinhados ao seed em <c>DbInitializer</c>.
/// </summary>
public static class ProdutoParametrosCatalogo
{
    public const string Categoria = "produto";

    /// <summary>Como o produto é vendido/faturado (ex.: UN, KIT, DZ, CX).</summary>
    public const string AtributoUnidadeComercializacao = "unidadeComercializacao";

    /// <summary>Natureza física do produto para NF-e (ex.: UN, KG, L, M).</summary>
    public const string AtributoUnidadeMedida = "unidadeMedida";

    /// <summary>Tipo de acondicionamento/embalagem (ex.: CX, FD, PCT, LAT).</summary>
    public const string AtributoUnidadeEmbalagem = "unidadeEmbalagem";

    /// <summary>Unidade das dimensões lineares — altura, largura, comprimento (ex.: CM, M, MM).</summary>
    public const string AtributoUnidadeDimensao = "unidadeDimensao";

    /// <summary>Unidade de massa logística — peso bruto (ex.: KG, G, T).</summary>
    public const string AtributoUnidadePeso = "unidadePeso";

    /// <summary>Origem geográfica (ex.: NACIONAL, IMPORTADO).</summary>
    public const string AtributoOrigemGeografica = "origemGeografica";

    /// <summary>Código de origem da mercadoria para ICMS — tabela SEFAZ (ex.: 0, 1, 2).</summary>
    public const string AtributoOrigemIcms = "origemIcms";
}
```

---

## 3. `DimensaoProduto.cs`

**Arquivo:** `DbMercado.Domain/Produto/ValueObjects/DimensaoProduto.cs`

Adicionar o campo `UnidadeDimensao` ao Value Object:

```csharp
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dimensões físicas do produto (sem embalagem comercial).
/// </summary>
public sealed class DimensaoProduto : IEquatable<DimensaoProduto>
{
    private DimensaoProduto() { }

    public decimal Altura { get; private set; }
    public decimal Largura { get; private set; }
    public decimal Comprimento { get; private set; }

    /// <summary>Unidade das medidas lineares (código do parâmetro: CM, M, MM).</summary>
    public string UnidadeDimensao { get; private set; } = string.Empty;

    public static DimensaoProduto Criar(decimal altura, decimal largura, decimal comprimento, string unidadeDimensao)
    {
        if (altura <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_ALTURA_INVALIDA", "Altura do produto deve ser maior que zero.");
        if (largura <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_LARGURA_INVALIDA", "Largura do produto deve ser maior que zero.");
        if (comprimento <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_COMPRIMENTO_INVALIDO", "Comprimento do produto deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(unidadeDimensao))
            throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_OBRIGATORIA", "Unidade das dimensões do produto é obrigatória.");

        return new DimensaoProduto
        {
            Altura = altura,
            Largura = largura,
            Comprimento = comprimento,
            UnidadeDimensao = unidadeDimensao.Trim().ToUpperInvariant()
        };
    }

    public void GarantirInvariantes()
    {
        GarantirPositivo(Altura, "PRODUTO_DIMENSAO_ALTURA_INVALIDA", "Altura do produto deve ser maior que zero.");
        GarantirPositivo(Largura, "PRODUTO_DIMENSAO_LARGURA_INVALIDA", "Largura do produto deve ser maior que zero.");
        GarantirPositivo(Comprimento, "PRODUTO_DIMENSAO_COMPRIMENTO_INVALIDO", "Comprimento do produto deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(UnidadeDimensao))
            throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_OBRIGATORIA", "Unidade das dimensões do produto é obrigatória.");
    }

    private static void GarantirPositivo(decimal valor, string codigo, string mensagem)
    {
        if (valor <= 0)
            throw new BusinessException(codigo, mensagem).With("ValorInformado", valor);
    }

    public bool Equals(DimensaoProduto? other) =>
        other is not null
        && Altura == other.Altura
        && Largura == other.Largura
        && Comprimento == other.Comprimento
        && string.Equals(UnidadeDimensao, other.UnidadeDimensao, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is DimensaoProduto d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Altura, Largura, Comprimento, UnidadeDimensao);
}
```

---

## 4. `DimensaoEmbalagem.cs`

**Arquivo:** `DbMercado.Domain/Produto/ValueObjects/DimensaoEmbalagem.cs`

Adicionar `UnidadeDimensao` e `UnidadePeso`:

```csharp
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dimensões e peso da embalagem logística do produto.
/// </summary>
public sealed class DimensaoEmbalagem : IEquatable<DimensaoEmbalagem>
{
    private DimensaoEmbalagem() { }

    public decimal Altura { get; private set; }
    public decimal Largura { get; private set; }
    public decimal Comprimento { get; private set; }
    public decimal Peso { get; private set; }

    /// <summary>Unidade das medidas lineares (código do parâmetro: CM, M, MM).</summary>
    public string UnidadeDimensao { get; private set; } = string.Empty;

    /// <summary>Unidade de massa (código do parâmetro: KG, G, T).</summary>
    public string UnidadePeso { get; private set; } = string.Empty;

    public static DimensaoEmbalagem Criar(
        decimal altura,
        decimal largura,
        decimal comprimento,
        decimal peso,
        string unidadeDimensao,
        string unidadePeso)
    {
        if (altura <= 0)
            throw new BusinessException("EMBALAGEM_ALTURA_INVALIDA", "Altura da embalagem deve ser maior que zero.");
        if (largura <= 0)
            throw new BusinessException("EMBALAGEM_LARGURA_INVALIDA", "Largura da embalagem deve ser maior que zero.");
        if (comprimento <= 0)
            throw new BusinessException("EMBALAGEM_COMPRIMENTO_INVALIDO", "Comprimento da embalagem deve ser maior que zero.");
        if (peso <= 0)
            throw new BusinessException("EMBALAGEM_PESO_INVALIDO", "Peso da embalagem deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(unidadeDimensao))
            throw new BusinessException("EMBALAGEM_UNIDADE_DIMENSAO_OBRIGATORIA", "Unidade das dimensões da embalagem é obrigatória.");
        if (string.IsNullOrWhiteSpace(unidadePeso))
            throw new BusinessException("EMBALAGEM_UNIDADE_PESO_OBRIGATORIA", "Unidade de peso da embalagem é obrigatória.");

        return new DimensaoEmbalagem
        {
            Altura = altura,
            Largura = largura,
            Comprimento = comprimento,
            Peso = peso,
            UnidadeDimensao = unidadeDimensao.Trim().ToUpperInvariant(),
            UnidadePeso = unidadePeso.Trim().ToUpperInvariant()
        };
    }

    public void GarantirInvariantes()
    {
        GarantirPositivo(Altura, "EMBALAGEM_ALTURA_INVALIDA", "Altura da embalagem deve ser maior que zero.");
        GarantirPositivo(Largura, "EMBALAGEM_LARGURA_INVALIDA", "Largura da embalagem deve ser maior que zero.");
        GarantirPositivo(Comprimento, "EMBALAGEM_COMPRIMENTO_INVALIDO", "Comprimento da embalagem deve ser maior que zero.");
        GarantirPositivo(Peso, "EMBALAGEM_PESO_INVALIDO", "Peso da embalagem deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(UnidadeDimensao))
            throw new BusinessException("EMBALAGEM_UNIDADE_DIMENSAO_OBRIGATORIA", "Unidade das dimensões da embalagem é obrigatória.");
        if (string.IsNullOrWhiteSpace(UnidadePeso))
            throw new BusinessException("EMBALAGEM_UNIDADE_PESO_OBRIGATORIA", "Unidade de peso da embalagem é obrigatória.");
    }

    private static void GarantirPositivo(decimal valor, string codigo, string mensagem)
    {
        if (valor <= 0)
            throw new BusinessException(codigo, mensagem).With("ValorInformado", valor);
    }

    public bool Equals(DimensaoEmbalagem? other) =>
        other is not null
        && Altura == other.Altura
        && Largura == other.Largura
        && Comprimento == other.Comprimento
        && Peso == other.Peso
        && string.Equals(UnidadeDimensao, other.UnidadeDimensao, StringComparison.Ordinal)
        && string.Equals(UnidadePeso, other.UnidadePeso, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is DimensaoEmbalagem d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Altura, Largura, Comprimento, Peso, UnidadeDimensao, UnidadePeso);
}
```

---

## 5. `ProdutoEntity.cs`

**Arquivo:** `DbMercado.Domain/Produto/Entities/ProdutoEntity.cs`

### 5.1 Substituir os campos de unidade

**Remover:**
```csharp
/// <summary>Unidade de medida (ex.: UN, KG, CX).</summary>
public string UnidadeMedida { get; private set; } = string.Empty;
```

**Adicionar no lugar:**
```csharp
/// <summary>Como o produto é vendido/faturado (código do parâmetro: UN, KIT, DZ, CX...).</summary>
public string UnidadeComercializacao { get; private set; } = string.Empty;

/// <summary>Natureza física para NF-e (código do parâmetro: UN, KG, L, M...).</summary>
public string UnidadeMedidaFisica { get; private set; } = string.Empty;

/// <summary>Tipo de acondicionamento da embalagem (código do parâmetro: CX, FD, PCT, LAT...).</summary>
public string TipoEmbalagem { get; private set; } = string.Empty;
```

### 5.2 Atualizar o método estático `Registrar`

**Remover o parâmetro:**
```csharp
string unidadeMedida,
```

**Adicionar no lugar:**
```csharp
string unidadeComercializacao,
string unidadeMedidaFisica,
string tipoEmbalagem,
```

**No corpo do método, substituir:**
```csharp
UnidadeMedida = CodigoUnidadeMedidaProduto.Criar(unidadeMedida).Codigo,
```

**Por:**
```csharp
UnidadeComercializacao = ValidarCodigoUnidade(unidadeComercializacao, "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA", "Unidade de comercialização é obrigatória."),
UnidadeMedidaFisica    = ValidarCodigoUnidade(unidadeMedidaFisica,    "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA",          "Unidade de medida física é obrigatória."),
TipoEmbalagem          = ValidarCodigoUnidade(tipoEmbalagem,           "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO",          "Tipo de embalagem é obrigatório."),
```

### 5.3 Atualizar `AtualizarDadosBasicos`

**Remover o parâmetro:**
```csharp
string unidadeMedida,
```

**Adicionar:**
```csharp
string unidadeComercializacao,
string unidadeMedidaFisica,
string tipoEmbalagem,
```

**No corpo, substituir:**
```csharp
UnidadeMedida = CodigoUnidadeMedidaProduto.Criar(unidadeMedida).Codigo;
```

**Por:**
```csharp
UnidadeComercializacao = ValidarCodigoUnidade(unidadeComercializacao, "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA", "Unidade de comercialização é obrigatória.");
UnidadeMedidaFisica    = ValidarCodigoUnidade(unidadeMedidaFisica,    "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA",          "Unidade de medida física é obrigatória.");
TipoEmbalagem          = ValidarCodigoUnidade(tipoEmbalagem,           "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO",          "Tipo de embalagem é obrigatório.");
```

### 5.4 Atualizar `Validar()`

**Remover:**
```csharp
_ = CodigoUnidadeMedidaProduto.Criar(UnidadeMedida);
```

**Adicionar:**
```csharp
ValidarCodigoUnidade(UnidadeComercializacao, "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA", "Unidade de comercialização é obrigatória.");
ValidarCodigoUnidade(UnidadeMedidaFisica,    "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA",          "Unidade de medida física é obrigatória.");
ValidarCodigoUnidade(TipoEmbalagem,           "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO",          "Tipo de embalagem é obrigatório.");
```

### 5.5 Adicionar método auxiliar privado

Adicionar no final da classe, junto dos outros métodos privados:

```csharp
private static string ValidarCodigoUnidade(string codigo, string errorCode, string errorMessage)
{
    if (string.IsNullOrWhiteSpace(codigo))
        throw new BusinessException(errorCode, errorMessage);

    var c = codigo.Trim().ToUpperInvariant();
    if (c.Length == 0 || c.Length > 16)
        throw new BusinessException(errorCode, errorMessage).With("CodigoInformado", codigo);

    foreach (var ch in c.AsSpan())
    {
        if (!char.IsLetterOrDigit(ch))
            throw new BusinessException(errorCode, errorMessage).With("CodigoInformado", codigo);
    }

    return c;
}
```

### 5.6 Remover a dependência do VO obsoleto

Após as alterações acima, o `CodigoUnidadeMedidaProduto` não será mais utilizado em
`ProdutoEntity`. Verificar se algum outro arquivo ainda o referencia antes de deletar.
Se não houver mais referências, **deletar o arquivo**:
`DbMercado.Domain/Produto/ValueObjects/CodigoUnidadeMedidaProduto.cs`

---

## 6. DTOs da Application

### 6.1 `ProdutoCreateDto.cs` e `ProdutoUpdateDto.cs`

**Remover:**
```csharp
public string UnidadeMedida { get; set; } = string.Empty;
```

**Adicionar:**
```csharp
/// <summary>Como o produto é vendido (código do parâmetro unidadeComercializacao).</summary>
public string UnidadeComercializacao { get; set; } = string.Empty;

/// <summary>Natureza física para NF-e (código do parâmetro unidadeMedida).</summary>
public string UnidadeMedidaFisica { get; set; } = string.Empty;

/// <summary>Tipo de acondicionamento (código do parâmetro unidadeEmbalagem).</summary>
public string TipoEmbalagem { get; set; } = string.Empty;
```

### 6.2 `ProdutoDimensaoDto.cs`

Adicionar a unidade:

```csharp
namespace DbMercado.Application.Produto.Dtos;

public class ProdutoDimensaoDto
{
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }

    /// <summary>Código do parâmetro unidadeDimensao (ex.: CM, M, MM).</summary>
    public string UnidadeDimensao { get; set; } = string.Empty;
}
```

### 6.3 `ProdutoDimensaoEmbalagemDto.cs`

Adicionar as unidades:

```csharp
namespace DbMercado.Application.Produto.Dtos;

public class ProdutoDimensaoEmbalagemDto
{
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }
    public decimal Peso { get; set; }

    /// <summary>Código do parâmetro unidadeDimensao (ex.: CM, M, MM).</summary>
    public string UnidadeDimensao { get; set; } = string.Empty;

    /// <summary>Código do parâmetro unidadePeso (ex.: KG, G, T).</summary>
    public string UnidadePeso { get; set; } = string.Empty;
}
```

### 6.4 `ProdutoResponseDto.cs` e `ProdutoLogisticaResponseDto.cs`

Verificar esses arquivos e substituir qualquer referência a `UnidadeMedida` pelos três
novos campos: `UnidadeComercializacao`, `UnidadeMedidaFisica`, `TipoEmbalagem`.
Nos DTOs de resposta de dimensão, acrescentar `UnidadeDimensao` e `UnidadePeso`.

---

## 7. `ProdutoEntityConfiguration.cs`

**Arquivo:** `DbMercado.Infrastructure/Produto/Persistence/Mappings/ProdutoEntityConfiguration.cs`

### 7.1 Substituir o mapeamento de `UnidadeMedida`

**Remover:**
```csharp
builder.Property(p => p.UnidadeMedida).IsRequired().HasMaxLength(16);
```

**Adicionar:**
```csharp
builder.Property(p => p.UnidadeComercializacao).IsRequired().HasMaxLength(16);
builder.Property(p => p.UnidadeMedidaFisica).IsRequired().HasMaxLength(16);
builder.Property(p => p.TipoEmbalagem).IsRequired().HasMaxLength(16);
```

### 7.2 Adicionar `UnidadeDimensao` e `UnidadePeso` nos OwnsOne

**No bloco `OwnsOne(p => p.DimensaoProduto)`**, adicionar:
```csharp
d.Property(x => x.UnidadeDimensao).IsRequired().HasMaxLength(8);
```

**No bloco `OwnsOne(p => p.DimensaoEmbalagem)`**, adicionar:
```csharp
d.Property(x => x.UnidadeDimensao).IsRequired().HasMaxLength(8);
d.Property(x => x.UnidadePeso).IsRequired().HasMaxLength(8);
```

---

## 8. `ProdutoService.cs`

**Arquivo:** `DbMercado.Application/Produto/Services/ProdutoService.cs`

Localizar todas as chamadas a `ProdutoEntity.Registrar(...)` e `AtualizarDadosBasicos(...)`
e atualizar para passar os três novos parâmetros de unidade no lugar do antigo `unidadeMedida`.

Fazer o mesmo nas chamadas a `DimensaoProduto.Criar(...)` e `DimensaoEmbalagem.Criar(...)`,
passando agora também `unidadeDimensao` e `unidadePeso` lidos dos respectivos DTOs.

---

## 9. `DbInitializer.cs`

**Arquivo:** `DbMercado.Infrastructure/Shared/Data/DbInitializer.cs`

### 9.1 Substituir o método `AddParametrosProduto` inteiro

```csharp
private static void AddParametrosProduto(AppDbContext context, DateTime dataCarga, string usuarioCarga)
{
    // ─── Unidade de comercialização — como o produto é vendido/faturado ───
    var unidadesCom = new[]
    {
        ("UN",  "Unidade"),
        ("KIT", "Kit"),
        ("DZ",  "Dúzia"),
        ("PAR", "Par"),
        ("CX",  "Caixa"),
        ("PCT", "Pacote"),
        ("FD",  "Fardo"),
        ("SC",  "Saco"),
        ("ROL", "Rolo"),
        ("M",   "Metro"),
        ("KG",  "Quilograma"),
        ("L",   "Litro"),
    };
    foreach (var (c, v) in unidadesCom)
        AddParametroIfNotExists(context, "produto", "unidadeComercializacao", c, v, null, dataCarga, usuarioCarga);

    // ─── Unidade de medida física — natureza do produto (NF-e / fiscal) ───
    var unidadesMedida = new[]
    {
        ("UN",  "Unidade"),
        ("KG",  "Quilograma"),
        ("G",   "Grama"),
        ("T",   "Tonelada"),
        ("L",   "Litro"),
        ("ML",  "Mililitro"),
        ("M",   "Metro"),
        ("CM",  "Centímetro"),
        ("MM",  "Milímetro"),
        ("M2",  "Metro quadrado"),
        ("M3",  "Metro cúbico"),
    };
    foreach (var (c, v) in unidadesMedida)
        AddParametroIfNotExists(context, "produto", "unidadeMedida", c, v, null, dataCarga, usuarioCarga);

    // ─── Unidade de embalagem — tipo de acondicionamento ───
    var unidadesEmb = new[]
    {
        ("CX",  "Caixa"),
        ("FD",  "Fardo"),
        ("PCT", "Pacote"),
        ("SC",  "Saco"),
        ("SAC", "Sacola"),
        ("LAT", "Lata"),
        ("GL",  "Galão"),
        ("FR",  "Frasco"),
        ("PT",  "Pote"),
        ("ROL", "Rolo"),
        ("TB",  "Tubo"),
        ("BL",  "Blister"),
    };
    foreach (var (c, v) in unidadesEmb)
        AddParametroIfNotExists(context, "produto", "unidadeEmbalagem", c, v, null, dataCarga, usuarioCarga);

    // ─── Unidade de dimensão — para altura, largura, comprimento ───
    var unidadesDim = new[]
    {
        ("CM", "Centímetro (cm)"),
        ("M",  "Metro (m)"),
        ("MM", "Milímetro (mm)"),
    };
    foreach (var (c, v) in unidadesDim)
        AddParametroIfNotExists(context, "produto", "unidadeDimensao", c, v, null, dataCarga, usuarioCarga);

    // ─── Unidade de peso — para peso bruto logístico ───
    var unidadesPeso = new[]
    {
        ("KG", "Quilograma (kg)"),
        ("G",  "Grama (g)"),
        ("T",  "Tonelada (t)"),
    };
    foreach (var (c, v) in unidadesPeso)
        AddParametroIfNotExists(context, "produto", "unidadePeso", c, v, null, dataCarga, usuarioCarga);

    // ─── Origem geográfica ───
    AddParametroIfNotExists(context, "produto", "origemGeografica", "NACIONAL",  "Nacional",  null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemGeografica", "IMPORTADO", "Importado", null, dataCarga, usuarioCarga);

    // ─── Origem ICMS — tabela SEFAZ completa (códigos 0 a 8) ───
    AddParametroIfNotExists(context, "produto", "origemIcms", "0", "0 — Nacional, exceto códigos 3 a 5",                                null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "1", "1 — Estrangeira, importação direta, exceto código 6",               null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "2", "2 — Estrangeira, adquirida no mercado interno, exceto código 7",    null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "3", "3 — Nacional, conteúdo de importação > 40% e ≤ 70%",               null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "4", "4 — Nacional, processo produtivo básico (PPB)",                    null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "5", "5 — Nacional, conteúdo de importação ≤ 40%",                       null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "6", "6 — Estrangeira, importação direta sem similar nacional",          null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "7", "7 — Estrangeira, mercado interno sem similar nacional",            null, dataCarga, usuarioCarga);
    AddParametroIfNotExists(context, "produto", "origemIcms", "8", "8 — Nacional, conteúdo de importação > 70%",                      null, dataCarga, usuarioCarga);
}
```

### 9.2 Remover o array estático obsoleto

**Deletar** o array `UnidadesMedidaProdutoSeed` que não será mais usado:

```csharp
// DELETAR todo este bloco:
private static readonly (string Codigo, string Rotulo)[] UnidadesMedidaProdutoSeed = { ... };
```

### 9.3 Atualizar os produtos de demonstração

Todas as chamadas a `ProdutoEntity.Registrar(...)` e `DimensaoEmbalagem.Criar(...)` no seed
precisam ser atualizadas com os novos parâmetros.

**Exemplo — antes:**
```csharp
ProdutoEntity.Registrar(
    nome: "Arroz parboilizado Tio João 1 kg",
    unidadeMedida: "UN",
    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.045m, 0.16m, 0.23m, 1.05m),
    ...
```

**Exemplo — depois:**
```csharp
ProdutoEntity.Registrar(
    nome: "Arroz parboilizado Tio João 1 kg",
    unidadeComercializacao: "UN",
    unidadeMedidaFisica: "KG",
    tipoEmbalagem: "PCT",
    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.045m, 0.16m, 0.23m, 1.05m, "CM", "KG"),
    dimensaoProduto: DimensaoProduto.Criar(0.04m, 0.15m, 0.22m, "CM"),
    ...
```

Aplicar o mesmo raciocínio para todos os produtos de demonstração:

| Produto | unidadeComercializacao | unidadeMedidaFisica | tipoEmbalagem |
|---|---|---|---|
| Arroz 1 kg | UN | KG | PCT |
| Azeite 500 ml | UN | ML | FR |
| Notebook 14" | UN | UN | CX |
| Detergente 500 ml | CX | L | CX |
| Café 250 g | UN | KG | PCT |
| Todos os extras de paginação | UN | UN | CX |

---

## 10. Migration EF Core

Após todas as alterações de código, gerar uma nova migration para refletir as mudanças
de schema no banco:

```bash
dotnet ef migrations add RefatoracaoUnidadesProduto \
  --project DbMercado.Infrastructure \
  --startup-project DbMercado.Api
```

Em seguida, aplicar:

```bash
dotnet ef database update \
  --project DbMercado.Infrastructure \
  --startup-project DbMercado.Api
```

> ⚠️ **Atenção:** A migration irá renomear colunas no banco. Se houver dados em produção,
> revisar o script gerado e, se necessário, adicionar instruções SQL de migração de dados
> antes de aplicar (ex.: copiar o valor antigo de `UnidadeMedida` para `UnidadeComercializacao`).

---

## 11. Resumo dos arquivos a alterar

| Arquivo | Tipo de alteração |
|---|---|
| `DbMercado.Application/Produto/ProdutoParametrosCatalogo.cs` | Substituição completa — adicionar 4 novas constantes |
| `DbMercado.Domain/Produto/ValueObjects/DimensaoProduto.cs` | Adicionar `UnidadeDimensao` + atualizar `Criar()` e `GarantirInvariantes()` |
| `DbMercado.Domain/Produto/ValueObjects/DimensaoEmbalagem.cs` | Adicionar `UnidadeDimensao` e `UnidadePeso` + atualizar `Criar()` e `GarantirInvariantes()` |
| `DbMercado.Domain/Produto/Entities/ProdutoEntity.cs` | Substituir `UnidadeMedida` por 3 campos + atualizar métodos |
| `DbMercado.Domain/Produto/ValueObjects/CodigoUnidadeMedidaProduto.cs` | **Deletar** (após confirmar que não há outras referências) |
| `DbMercado.Application/Produto/Dtos/ProdutoCreateDto.cs` | Substituir `UnidadeMedida` por 3 campos |
| `DbMercado.Application/Produto/Dtos/ProdutoUpdateDto.cs` | Idem |
| `DbMercado.Application/Produto/Dtos/ProdutoDimensaoDto.cs` | Adicionar `UnidadeDimensao` |
| `DbMercado.Application/Produto/Dtos/ProdutoDimensaoEmbalagemDto.cs` | Adicionar `UnidadeDimensao` e `UnidadePeso` |
| `DbMercado.Application/Produto/Dtos/ProdutoResponseDto.cs` | Atualizar campos de unidade |
| `DbMercado.Application/Produto/Dtos/ProdutoLogisticaResponseDto.cs` | Atualizar campos de unidade |
| `DbMercado.Application/Produto/Services/ProdutoService.cs` | Atualizar chamadas a `Registrar`, `AtualizarDadosBasicos`, `DimensaoEmbalagem.Criar`, `DimensaoProduto.Criar` |
| `DbMercado.Infrastructure/Produto/Persistence/Mappings/ProdutoEntityConfiguration.cs` | Remapear colunas |
| `DbMercado.Infrastructure/Shared/Data/DbInitializer.cs` | Substituir `AddParametrosProduto` + deletar array obsoleto + atualizar seed de produtos |
| **Nova migration EF** | Criar e aplicar |
