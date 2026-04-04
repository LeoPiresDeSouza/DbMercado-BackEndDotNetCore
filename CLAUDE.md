# DbMercado — Contexto do Backend (.NET)

> Arquivo lido automaticamente pelo Claude Code e pelo Cursor.
> Coloque na raiz do repositório backend (onde está o DbMercado.sln).
> Última atualização: inspecionado ao vivo em 04/04/2026.

---

## 1. O Projeto

API REST em .NET Core 9 para o sistema DbMercado — gestão de estoque
e fulfillment. Veja o contexto de negócio completo no CLAUDE.md do frontend.

**Porta:** 5046
**Solution:** DbMercado.sln (na raiz do repositório)

---

## 2. Arquitetura — Clean Architecture em 4 camadas

```
DbMercado.Domain          → Entidades, Value Objects, interfaces, exceções de negócio
DbMercado.Application     → Casos de uso, DTOs, interfaces de serviço, mapeamentos
DbMercado.Infrastructure  → Repositórios EF, jobs Quartz, storage, integrações externas
DbMercado.Api             → Controllers, middlewares, DI, configuração (Program.cs)
```

### Regras entre camadas
- Domain não referencia nenhuma outra camada
- Application referencia apenas Domain
- Infrastructure referencia Domain e Application
- Api referencia Application e Infrastructure (apenas para DI)
- Controllers chamam apenas Application services — nunca repositórios diretamente

---

## 3. Bounded Contexts — Estado atual

| Contexto         | Status     | Tabelas principais |
|------------------|------------|--------------------|
| Administracao    | ✅ Funcional | dbParametro, Identity tables |
| Produto          | ✅ Funcional | prdProduto, prdSku, prdProdutoAtributo, prdCategoria, prdMidia |
| Importacao       | ✅ Funcional | impNotaFiscal, impItemNotaFiscal, impProdutoImportado |
| Midia            | ✅ Funcional | prdMidia |
| Financeiro       | 🔲 Vazio   | — |
| Logistica        | 🔲 Vazio   | — |
| Marketplace      | 🔲 Vazio   | — |
| Pedidos          | 🔲 Vazio   | — |

### Nomenclatura obrigatória de tabelas
- `db` → Administração/configuração (ex: dbParametro, dbAppLog)
- `prd` → Produto (ex: prdProduto, prdCategoria, prdMidia)
- `imp` → Importação (ex: impNotaFiscal)
- Novos contextos seguirão prefixos próprios (ex: `log` Logística, `fin` Financeiro)

---

## 4. Todos os Endpoints (inspecionados ao vivo)

```
Auth:
  POST   /api/Auth/login
  POST   /api/Auth/refresh
  POST   /api/Auth/logout
  POST   /api/Auth/revoke

Módulos do usuário:
  GET    /api/Modulo/modulosUsuario

Categorias de produto:
  GET    /api/categorias-produto          → árvore completa aninhada
  POST   /api/categorias-produto          → criar categoria
  PUT    /api/categorias-produto/{id}     → atualizar nome/descrição
  DELETE /api/categorias-produto/{id}     → inativar (sem filhas/produtos)

Importação:
  POST   /api/importacao/notas-fiscais
  GET    /api/importacao/notas-fiscais
  GET    /api/importacao/notas-fiscais/{id}
  POST   /api/importacao/produtos-importados
  GET    /api/importacao/produtos-importados
  GET    /api/importacao/produtos-importados/{id}

Mídias:
  POST   /api/produtos/midia/upload       → upload individual, retorna midiaId+url
  DELETE /api/produtos/midia/{midiaId}    → remove mídia temporária

Produtos:
  POST   /api/produtos                    → criar produto
  GET    /api/produtos                    → listar
  PUT    /api/produtos/{id}               → atualizar produto
  DELETE /api/produtos/{id}               → excluir
  GET    /api/produtos/{id}               → detalhe completo
  GET    /api/produtos/{id}/midias        → listar mídias do produto
  POST   /api/produtos/{id}/midias        → associar mídias (temp → ativo)
  GET    /api/produtos/{id}/logistica     → dados logísticos do produto

Parâmetros de produto (listas para selects):
  GET    /api/produtos/parametros/unidades-comercializacao
  GET    /api/produtos/parametros/unidades-medida
  GET    /api/produtos/parametros/tipos-embalagem
  GET    /api/produtos/parametros/unidades-dimensao
  GET    /api/produtos/parametros/unidades-peso
  GET    /api/produtos/parametros/origens-geograficas
  GET    /api/produtos/parametros/origens-icms

Consultas de produto:
  POST   /api/produtos/consultas/grid     → SSRM AG Grid
  GET    /api/produtos/consultas/por-ncm
  GET    /api/produtos/consultas/por-origem
  GET    /api/produtos/consultas/por-unidade-medida
  GET    /api/produtos/consultas/por-marca

Test (dev only):
  GET    /api/Test/public|authenticated|admin-only|menu-manager|read-permission
```

---

## 5. Todos os DTOs (inspecionados ao vivo)

```
Auth:
  LoginRequest:          [email, password]
  RefreshTokenRequest:   [refreshToken]

Categorias:
  CategoriaCreateDto:    [nome, descricao, categoriaPaiId]
  CategoriaUpdateDto:    [nome, descricao]
  CategoriaTreeNodeDto:  [id, nome, slug, descricao, categoriaPaiId, nivel, ativo, subcategorias]

Importação:
  NotaFiscalCadastroRequest:      [chaveAcesso, numero, serie, dataEmissao, cnpjEmitente,
                                   razaoSocialEmitente, valorTotal, itens]
  NotaFiscalItemCadastroRequest:  [numeroItem, codigoProdutoFornecedor, descricao, ncm,
                                   quantidade, valorUnitario, valorTotalLinha]
  NotaFiscalResponse:             [id, chaveAcesso, numero, serie, dataEmissao, cnpjEmitente,
                                   razaoSocialEmitente, valorTotal, itens]
  NotaFiscalResumoResponse:       [id, chaveAcesso, numero, dataEmissao, valorTotal]
  ItemNotaFiscalResponse:         [id, numeroItem, codigoProdutoFornecedor, descricao, ncm,
                                   quantidade, valorUnitario, valorTotalLinha]
  ProdutoImportadoCadastroRequest:[codigoInterno, descricao, ncm, unidadeMedida,
                                   itemNotaFiscalOrigemId]
  ProdutoImportadoResponse:       [id, codigoInterno, descricao, ncm, unidadeMedida,
                                   itemNotaFiscalOrigemId, notaFiscalOrigemId,
                                   notaFiscalChaveAcesso]
  ProdutoImportadoResumoResponse: [id, codigoInterno, descricao]

Mídias:
  MidiaUploadResponseDto:  [midiaId, url, thumbnailUrl]
  MidiaResponseDto:        [id, url, thumbnailUrl, tipo, ordem, isPrincipal, duracao, status]
  MidiaAssociarDto:        [itens]  ← required
  MidiaAssociarItemDto:    [midiaId, isPrincipal]

Produtos — Create/Update:
  ProdutoCreateDto:        [nome, descricao, marca, modelo, gtin, categoriaProdutoId,
                            unidadeComercializacao, unidadeMedidaFisica, tipoEmbalagem,
                            origemGeografica, dadosFiscais, dimensaoProduto,
                            dimensaoEmbalagem, skus, atributos, midias]
  ProdutoUpdateDto:        (mesmos campos do Create)

Produtos — Response:
  ProdutoResponseDto:      [id, nome, descricao, marca, modelo, gtin,
                            categoriaProdutoId, categoriaNome, categoriaSlug, categoriaCaminho,
                            unidadeComercializacao, unidadeMedidaFisica, tipoEmbalagem,
                            origemGeograficaTipo, origemGeograficaPais,
                            dadosFiscais, dimensaoProduto, dimensaoEmbalagem,
                            skus, atributos]
  ProdutoListItemDto:      [id, nome, marca, modelo, gtin, unidadeComercializacao,
                            unidadeMedidaFisica, tipoEmbalagem, ncm,
                            origemGeograficaTipo, origemGeograficaPais]
  ProdutoResumoDto:        [id, nome, unidadeComercializacao, unidadeMedidaFisica,
                            tipoEmbalagem, marca]
  ProdutoLogisticaResponseDto: [dimensaoProduto, dimensaoEmbalagem, unidadeComercializacao,
                                unidadeMedidaFisica, tipoEmbalagem]

Produtos — Sub-DTOs:
  ProdutoOrigemDto:            [tipo, paisOrigem]
  ProdutoDadosFiscaisDto:      [ncm, cest, origem]
  ProdutoDimensaoDto:          [altura, largura, comprimento, peso, unidadeDimensao, unidadePeso]
  ProdutoDimensaoEmbalagemDto: [altura, largura, comprimento, peso, unidadeDimensao, unidadePeso]
  ProdutoSkuItemDto:           [codigo, ativo]
  ProdutoSkuResponseDto:       [id, codigo, ativo]
  ProdutoAtributoDto:          [nome, valor]
  ProdutoUnidadeMedidaOpcaoDto:[codigo, rotulo]

Produtos — Grid SSRM:
  ProdutoGridQueryDto:     [startRow, endRow, sortModel, filterModel, rowGroupCols,
                            groupKeys, valueCols, pivotMode, categoriaIdFiltro, origemFiltro]
  ProdutoGridResultDto:    [rows, rowCount]
  ProdutoGridRowDto:       [isGroup, id, nome, unidadeComercializacao, unidadeMedidaFisica,
                            tipoEmbalagem, marca, categoriaNome, categoriaSlug,
                            childCount, groupKey]
  ProdutoGridSortItemDto:  [colId, sort]
  ProdutoGridColumnVoDto:  [id, displayName, field, aggFunc]

Usuários/Módulos:
  ModuloUsuarioResponse:         [moduloId, nomeNormalizado, nomeExibicao, ordemExibicao,
                                  icone, funcionalidades]
  FuncionalidadeUsuarioResponse: [funcionalidadeId, nomeNormalizado, nomeExibicao,
                                  ordemExibicao, icone, permissoes]
  PermissaoUsuarioResponse:      [permissaoId, permissao]
```

---

## 6. Padrões de código obrigatórios

### Entidades (Domain)
```csharp
// Setters SEMPRE privados
public string Nome { get; private set; } = string.Empty;

// Mutações via métodos de domínio
public void Atualizar(string nome, string usuarioAuditoria)
{
    Nome = nome.Trim();
    DataUltimaAlteracao = DateTime.UtcNow;
    UsuarioUltimaAlteracao = usuarioAuditoria;
}

// Construtor privado para materialização pelo ORM
private MinhaEntidade() { }

// Factory method para criação
public static MinhaEntidade Criar(...) { ... }
```

### Exceções de negócio
```csharp
// Sempre BusinessException com código + mensagem legível
throw new BusinessException("PRODUTO_NOME_OBRIGATORIO", "Nome do produto é obrigatório.");

// Com dados adicionais para diagnóstico
throw new BusinessException("PRODUTO_SKU_DUPLICADO", $"SKU duplicado: '{codigo}'.")
    .With("Codigo", codigo);

// Entidade duplicada
throw new DuplicateEntityException("CODIGO", "Mensagem").With("Campo", valor);
```

### BaseEntity (herdar em todas as entidades)
```csharp
// Campos disponíveis via herança:
public byte[]? RowVersion { get; set; }
public DateTime DataCriacao { get; set; }
public DateTime DataUltimaAlteracao { get; set; }
public string UsuarioCriacao { get; set; }
public string UsuarioUltimaAlteracao { get; set; }
```

### Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeuController : ControllerBase
{
    // Injetar apenas Application services — nunca repositórios
    // Retornar ProblemDetails em erros (já tratado pelo middleware)
    // Usar CancellationToken em todos os métodos async
}
```

### EntityTypeConfiguration (EF Core)
```csharp
// Sempre usar IEntityTypeConfiguration<T>
// Mapear tabela com prefixo correto
builder.ToTable("prdMinhaEntidade");

// Owned types para Value Objects
builder.OwnsOne(p => p.MeuValueObject, vo => {
    vo.Property(x => x.Campo).IsRequired().HasMaxLength(128);
});
```

---

## 7. Estrutura de pastas por bounded context

Ao criar um novo contexto (ex: Logistica), seguir:

```
DbMercado.Domain/Logistica/
  Entities/            → entidades com setters privados
  Interfaces/
    Repositories/      → IXxxRepository
    UnitsOfWork/       → IUwLogistica
  ValueObjects/        → VOs imutáveis
  Enums/
  Constants/           → códigos de status, etc.

DbMercado.Application/Logistica/
  Services/            → casos de uso
  Dtos/                → Request/Response DTOs
  Interfaces/          → IXxxService
  Mapping/             → AutoMapper profiles (se usado)

DbMercado.Infrastructure/Logistica/
  Repositories/        → implementações EF
  UnitsOfWork/
  Persistence/
    Mappings/          → IEntityTypeConfiguration

DbMercado.Api/Controllers/Logistica/
  → Controller com [Authorize], verbos REST, CancellationToken
```

---

## 8. Jobs Quartz — Estrutura atual

```
Infrastructure/Jobs/
  Abstractions/
    ScopedBackgroundJob.cs    → classe base — cria escopo DI por execução
  Configuration/
    QuartzSchedulingOptions.cs → options: LogCleanup, LimpezaMidias,
                                  MarketplaceSync (reservado),
                                  ConciliacaoFinanceira (reservado),
                                  Reprocessamento (reservado)
  DependencyInjection/
    ServiceCollectionQuartzExtensions.cs → registrar novos jobs aqui
  Maintenance/
    LogCleanupJob.cs               → ✅ limpa dbAppLog
    LimpezaMidiasTemporariasJob.cs → ✅ limpa prdMidia status=temporario
  Marketplace/   → .gitkeep (reservado)
  Financeiro/    → .gitkeep (reservado)
  Reprocessamento/ → .gitkeep (reservado)
```

### Para adicionar novo job
1. Criar `MeuJob.cs` herdando `ScopedBackgroundJob`
2. Adicionar options em `QuartzSchedulingOptions`
3. Registrar em `ServiceCollectionQuartzExtensions`
4. Configurar em `appsettings.json`

---

## 9. Storage de mídia

```
IStorageService (Domain/Produto/Interfaces/)
  SalvarAsync(stream, nome, contentType) → url
  DeletarAsync(urlOuCaminho)             → void (melhor esforço)
  ExisteAsync(urlOuCaminho)              → bool

LocalStorageService (Infrastructure/Produto/Storage/)
  Implementação V1 — salva em wwwroot/uploads/produtos/{ano}/{mes}/{guid}{ext}
  Organiza por ano/mês para evitar excesso de arquivos numa pasta

⚠️ Sempre usar IStorageService — nunca acoplar lógica de arquivo ao controller
```

---

## 10. Configuração (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "JwtSettings": {
    "SecretKey": "...",
    "ExpirationHours": 8
  },
  "Storage": {
    "Local": {
      "Pasta": "uploads/produtos",
      "UrlBase": "/uploads/produtos"
    }
  },
  "Quartz": {
    "SchedulerName": "DbMercadoScheduler",
    "WaitForJobsToComplete": true,
    "AwaitApplicationStarted": true,
    "LogCleanup": {
      "Enabled": true,
      "CronSchedule": "0 0 3 * * ?",
      "RetentionDays": 90
    },
    "LimpezaMidias": {
      "Enabled": true,
      "CronSchedule": "0 0 2 * * ?",
      "RetencaoHoras": 24,
      "TamanhoBatch": 100
    }
  }
}
```

---

## 11. Comandos

```bash
# Rodar API
dotnet run --project DbMercado.Api --launch-profile http

# Migrations
dotnet ef migrations add [Nome] \
  --project DbMercado.Infrastructure \
  --startup-project DbMercado.Api

dotnet ef database update \
  --project DbMercado.Infrastructure \
  --startup-project DbMercado.Api
```

---

## 12. O que NÃO fazer

- ❌ Connection string hardcoded
- ❌ Lógica de negócio em Controllers
- ❌ Entidades com setters públicos
- ❌ Integrações externas no Domain
- ❌ Repositórios injetados em Controllers
- ❌ Base64 para mídia
- ❌ Buffer binário no banco
- ❌ Nomenclatura em inglês (usar português em tabelas, DTOs, campos)
- ❌ Criar novo endpoint sem seguir o padrão Controller → Service → Repository
- ❌ Acoplar lógica de arquivo ao controller (usar IStorageService)

---

## 13. Roadmap pendente

### Próximos bounded contexts a implementar (ordem sugerida)
1. **Logistica** — OperadorLogistico, endereços, capacidade
2. **Clientes** — PJ/PF, vínculos com operadores
3. **Estoque** — saldo por operador, movimentações entrada/saída
4. **Compras** — pedido de compra → recebimento → estoque
5. **Pedidos** — pedido de venda → operador logístico
6. **Marketplace** — integração fulfillment
7. **Financeiro** — contas a pagar (compras, frete) e receber (vendas)
