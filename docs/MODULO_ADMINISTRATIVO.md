# Módulo Administrativo — escopo, modelo e gaps

Este documento detalha o **bounded context Administrativo** do backend .NET, alinhado ao documento estratégico da *Inventory Allocation Platform*. Serve como referência para evolução incremental e para prompts no Cursor (uma tarefa por vez).

---

## 1. Responsabilidade do contexto

Governança e cadastros transversais ao negócio operacional:

- Identidade e autenticação (ASP.NET Identity + JWT na API).
- Cadastro de **pessoas** (PF/PJ) e vínculo com usuário de aplicação.
- **Permissões** por módulo, funcionalidade e permissão (claims no Identity).
- **Parâmetros** configuráveis (chave/valor em banco).
- **Logs** de aplicação persistidos (`AppLogEntity` / tabela `dbAppLog`).
- Utilitários de apoio expostos à aplicação (ex.: consulta de CEP via providers).

Não pertencem aqui: nota fiscal, estoque, pedidos, financeiro, integrações de marketplace (outros contextos).

---

## 2. Mapa de entidades (Domain)

| Entidade | Papel | Observação |
|----------|--------|------------|
| `PessoaEntity` | Raiz de agregado de cadastro | PF/PJ; `TipoPessoa`; documento via value object |
| `PessoaFisicaEntity` / `PessoaJuridicaEntity` | Detalhes por tipo | Ligadas a `PessoaEntity` |
| `PessoaEmailEntity`, `PessoaTelefoneEntity`, `PessoaEnderecoEntity`, `PessoaRedeSocialEntity` | Filhas do cadastro | |
| `UsuarioEntity` | Usuário de negócio | Relaciona com pessoa / Identity |
| `ModuloEntity`, `FuncionalidadeEntity`, `PermissaoEntity` | Árvore de menu e autorização | Usado com Identity claims |
| `ParametroEntity` | Parâmetros dinâmicos | |
| `RefreshTokenEntity` | Refresh token JWT | |
| `AppLogEntity` | Log persistido | Namespace `Domain.Shared` (kernel), tabela de log |

**Identity:** `IdentityUser` / `IdentityRole` geridos pelo EF (não são entidades de domínio próprias).

**Value objects / enums (Administracao):** `Documento`, `IdentityUsuarioAdd`, `IdentityUserClaims`, `TipoPessoa`, `TipoPessoaPersistencia`, `TipoDocumentoFiscal`.

---

## 3. Fluxos principais (comportamento atual)

### 3.1 Autenticação

- Login gera access token (JWT) e refresh token; refresh revoga tokens antigos e emite novos.
- Configuração: `JwtSettings` + Identity na API.
- Serviços: `IAuthService` / `AuthService` (Application), `IAuthenticateService` / `AuthenticateService` (Infrastructure).

### 3.2 Permissões e menu

- Endpoint que retorna módulos/funcionalidades/permissões por **nome de usuário** (Identity).
- `IModuloService` / `ModuloService`; `IModuloRepository` com cache por entidade (`IApplicationCachingService<ModuloEntity>`).

### 3.3 Pessoas e usuários

- Modelo rico em `Administracao/Entities` com regras em entidades (documento, tipo, etc.).
- Persistência via `AppDbContext`; concorrência otimista em `PessoaEntity` e `UsuarioEntity` (entre outras administrativas).

### 3.4 Parâmetros

- CRUD via repositório base / contexto (sem serviço dedicado na Application listado de forma isolada — evoluir se necessário).

### 3.5 Logs

- `DatabaseLoggerProvider` grava em `dbAppLog`.
- Job `LogCleanupJob` (Quartz) remove registros antigos conforme `appsettings` (`Quartz:LogCleanup`).

### 3.6 CEP

- `ICepService` / `CepService`; providers `ViaCep` / `BrasilApi` com Polly na Infrastructure.

---

## 4. Camadas e arquivos de referência

| Camada | Local (padrão atual) |
|--------|-------------------------|
| Domain | `DbMercado.Domain/Administracao/`, `Domain/Shared/` (log, base, exceções) |
| Application | `DbMercado.Application/Administracao/` (DTOs, interfaces, services, mappers) |
| Infrastructure | `DbMercado.Infrastructure/Administracao/` (repositórios, mappings, UoW, autenticação) |
| API | `DbMercado.Api/Controllers/Administracao/`, `Autenticacao/` |

**Unit of Work:** `IUwAdministracao` / `UwAdministracao` — `ModuloRepository`, `UsuarioIdentityRepository`, `RefreshTokenRepository`.

---

## 5. Concorrência otimista e auditoria

- `BaseEntity`: `RowVersion`, datas e usuários de criação/alteração.
- `AppDbContext.ConfigureOptimisticConcurrency`: `RowVersion` ativo para tipos administrativos definidos explicitamente (ex.: `PessoaEntity`, `UsuarioEntity`, `ModuloEntity`, `ParametroEntity`); demais herdeiros de `BaseEntity` ignoram `RowVersion` no modelo até decisão de negócio.

---

## 6. Gaps e backlog sugerido

Itens úteis que ainda **não** estão descritos como “fechados” no código/guia:

1. **Documentação de API** (OpenAPI) para todos os endpoints administrativos e contratos de erro padronizados.
2. **Serviço de aplicação explícito para Parâmetros** (se o front depender de CRUD rico).
3. **Gestão de usuários no domínio de negócio** além do Identity (convites, vínculo pessoa–usuário em fluxo único de API).
4. **Auditoria de alterações** (quem mudou o quê) além dos campos em `BaseEntity`, se exigido por compliance.
5. **Testes automatizados** de serviços críticos (auth, módulos, pessoa).
6. **Políticas de autorização** por permissão (além de `[Authorize]`), alinhadas a `PermissaoEntity`.

Evitar neste contexto: regras de estoque, pedido ou NF (Importação / outros BCs).

---

## 7. Como pedir evoluções no Cursor (exemplos)

- “Adicionar endpoint administrativo de listagem paginada de `ParametroEntity` com DTO e autorização.”
- “Incluir política baseada em claim para `ModuloController`.”
- “Estender `PessoaEntity` com validação de X sem misturar regras de Importação.”

Evitar: “implementar ERP administrativo completo” num único prompt.

---

## 8. Relação com as fases do roadmap

O módulo Administrativo corresponde à **Fase 1 — Fundação** do documento estratégico. O backend atual cobre a maior parte dessa fase; os itens da seção 6 são refinamentos e produto, não mudança de direção arquitetural.

---

## 9. Próximo contexto no backend

A **Fase 2 — Importação** está implementada no código em `Domain/Importacao`, `Application/Importacao`, `Infrastructure/Importacao` e `Api/Controllers/Importacao`, com tabelas `impNotaFiscal`, `impItemNotaFiscal` e `impProdutoImportado` (migration `Fase2_Importacao`). Convenções para clientes HTTP externos estão em [CONVENCOES_INTEGRACOES.md](./CONVENCOES_INTEGRACOES.md).
