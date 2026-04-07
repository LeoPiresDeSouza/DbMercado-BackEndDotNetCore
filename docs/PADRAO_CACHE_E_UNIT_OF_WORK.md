# Padrão obrigatório: cache de aplicação e Unit of Work

> **Ponto fixo do projeto (backend).** Toda análise de arquitetura, especificação (`.md` de etapas) e implementação que envolva **persistência EF Core**, **leituras que possam ser cacheadas** ou **novos bounded contexts** deve verificar explicitamente este documento **antes** de fechar desenho ou código.

Documentação complementar e exemplos: `backEndDotNetCore/CLAUDE.md` (seção *Cache em memória* e estrutura de pastas por contexto).

---

## 1. Por que isso existe

- O `ApplicationCachingService<TEntity>` segrega chaves por tipo de entidade e invalida em lote via token (`InvalidateEntity()`).
- Se uma escrita passar por `SaveChanges` **sem** passar pela UoW que chama `UnitOfWorkCacheInvalidacao`, leituras cacheadas podem ficar **stale**.
- Registrar repositórios com `AddScoped<IRepo, Repo>()` **fora** da fábrica/UoW facilita esquecer o encadeamento **contexto único + invalidação**.

---

## 2. `ApplicationCachingService` / `IApplicationCachingFactory`

| Item | Onde |
|------|------|
| Implementação | `DbMercado.Infrastructure/Providers/Caching/ApplicationCachingService.cs` (namespace `DeepBlues.Infrastructure.Providers`) |
| Interface | `DbMercado.Infrastructure/Shared/Interfaces/IApplicationCachingService.cs` |
| Factory | `ApplicationCachingFactory` + `IApplicationCachingFactory` |
| Registro DI | `Program.cs` — `AddTransient(typeof(IApplicationCachingService<>), typeof(ApplicationCachingService<>))` e `IApplicationCachingFactory` |

**Uso típico em leitura:** `GetOrCreateAsync(CachePolicy, chave, factory, ...)` — escolher política (`LongTerm` / `MediumTerm` / `ShortTerm`) conforme volatilidade (ver `CLAUDE.md`).

**Invalidação:** feita **após** `SaveChangesAsync` bem-sucedido na UoW, via `UnitOfWorkCacheInvalidacao.AposSaveSeAlterou` — **não** chamar `InvalidateEntity()` diretamente dos serviços da Application como regra geral.

**O que normalmente não entra neste cache:** tokens de refresh, grids de auditoria, listagens altamente dinâmicas, qualquer dado que precise refletir escrita imediata (documentar exceção se houver).

---

## 3. Unit of Work (`IUw*` / `Uw*`)

**Padrão obrigatório** para bounded contexts que possuem repositórios EF e participam do cache:

1. **Interface** em `DbMercado.Domain/<Contexto>/Interfaces/UnitsOfWork/IUw<Contexto>.cs` expondo repositórios e:
   - `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);`
2. **Implementação** em `DbMercado.Infrastructure/<Contexto>/UnitsOfWork/Uw<Contexto>.cs`:
   - Um único `AppDbContext` injetado (mesma instância do request).
   - `IRepositoryFactory` + `IApplicationCachingFactory` injetados.
   - Repositórios obtidos com `_repoFactory.Create<ImplementacaoRepository>(_context)` (lazy), **não** `new` manual salvo casos excepcionais já padronizados (ex.: `RefreshTokenRepository` na factory).
   - `SaveChangesAsync` deve:
     ```csharp
     var linhas = await _context.SaveChangesAsync(cancellationToken);
     UnitOfWorkCacheInvalidacao.AposSaveSeAlterou(
         _cacheFactory,
         linhas,
         UnitOfWorkCacheInvalidacao.<Contexto>);
     return linhas;
     ```
3. **Application services** usam `IUw*` e chamam `SaveChangesAsync(cancellationToken)` — evitar `SaveChanges` direto no `AppDbContext` no service quando o contexto usa cache invalidado pela UoW.

4. **`UnitOfWorkCacheInvalidacao.cs`:** ao criar novo bounded context (ou novas entidades cacheadas num contexto existente), **atualizar** ou criar método estático agregando `GetApplicationCaching<TEntity>().InvalidateEntity()` para cada `TEntity` relevante.

---

## 4. `RepositoryFactory` e construtores dos repositórios

- Repositórios que **herdam** `BaseRepository<TEntity>` recebem `(AppDbContext, IApplicationCachingService<TEntity>)` via reflexão na factory (tipo genérico inferido da base).
- Repositórios que **não** herdam `BaseRepository<>` mas participam do mesmo padrão: construtor com `AppDbContext` + `IApplicationCachingService<TEntity>` (o segundo parâmetro pode ser `_` se ainda não houver leitura cacheada, **somente** para manter a cadeia de criação e invalidação) — a factory resolve via `ActivatorUtilities.CreateInstance(..., context)` e o restante pelo `IServiceProvider`.
- **Exceções intencionais** (hoje): repositórios registrados direto no `Program.cs` sem UoW da mesma forma — ex.: `AppLog`, `JobExecucao`, parte de infraestrutura. **Nova exceção** só com justificativa escrita (neste arquivo ou no PR) e preferencialmente listada na seção 6 abaixo.

---

## 5. Checklist antes de merge (novo módulo ou persistência)

- [ ] Existe `IUw*` com `SaveChangesAsync(CancellationToken)`?
- [ ] Repositórios do contexto são criados pela UoW + `IRepositoryFactory` (e não há `AddScoped` duplicado para o mesmo repositório sem motivo)?
- [ ] `UnitOfWorkCacheInvalidacao` cobre todas as entidades `TEntity` para as quais existe `GetOrCreate` / `Set` com token?
- [ ] Serviços da Application não persistem com `AppDbContext.SaveChanges` direto quando o módulo invalida cache pela UoW?
- [ ] Leituras que não devem ser cacheadas estão explícitas (comentário ou doc)?

---

## 6. Exceções conhecidas (manter atualizado)

| Repositório / fluxo | Motivo |
|---------------------|--------|
| `IAppLogRepository` / `IJobExecucaoRepository` | Leituras operacionais/auditoria; sem UoW de negócio compartilhada com catálogo |
| `RefreshTokenRepository` | Caminho especial na `RepositoryFactory`; consistência imediata |
| `ParametroChaveConsultaRepository` | Avaliar alinhamento futuro; hoje usa cache com ctor próprio |

---

## 7. Chat (e outros contextos com PK `Guid`)

O `BaseRepository<T>` histórico assume operações com PK `long`; entidades de chat usam `Guid`. Isso **não** impede o padrão UoW + invalidação:

**Recomendação imediata**

1. Criar `IUwChat` / `UwChat` como nas seções 3–4.
2. Remover `AddScoped<IChatRoomRepository, …>` e `AddScoped<IMessageRepository, …>` do `Program.cs`; expor os repositórios **apenas** pela `IUwChat`.
3. Alterar `ChatRoomRepository` e `MessageRepository` para receber `(AppDbContext context, IApplicationCachingService<ChatRoomEntity> _)` e `(AppDbContext, IApplicationCachingService<MessageEntity> _)` respectivamente (ou um serviço por entidade que for cacheada de fato).
4. Em `UnitOfWorkCacheInvalidacao`, adicionar método estático `Chat` invalidando `ChatRoomEntity`, `MessageEntity`, `MessageTranslationEntity` e demais entidades do módulo que vierem a usar `GetOrCreate`/`Set` com política de cache.
5. Serviços/hub (quando existirem) injetam `IUwChat` e chamam `SaveChangesAsync(cancellationToken)` após operações de escrita.

**Leituras em tempo real (SignalR):** grande parte das consultas de mensagens pode permanecer **sem** cache (`AsNoTracking` direto). Mesmo assim, manter a UoW e os construtores com `IApplicationCachingService<TEntity>` (mesmo descartado) garante **um único desenho** de persistência e invalidação quando alguém introduzir cache (ex.: lista de salas do usuário com `MediumTerm`).

---

## 8. Referências rápidas de código

- `DbMercado.Infrastructure/Shared/UnitsOfWork/UnitOfWorkCacheInvalidacao.cs`
- `DbMercado.Infrastructure/Shared/Repositories/RepositoryFactory.cs`
- `DbMercado.Infrastructure/Produto/UnitsOfWork/UwProduto.cs` (referência de `SaveChangesAsync` + invalidação)
- `DbMercado.Infrastructure/Importacao/UnitsOfWork/UwImportacao.cs`
