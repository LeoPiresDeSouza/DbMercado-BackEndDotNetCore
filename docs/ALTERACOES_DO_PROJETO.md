# Registro de alterações do projeto DbMercado API

Este documento descreve as alterações realizadas no projeto ao longo das melhorias de arquitetura, configuração e observabilidade. A documentação detalhada do **middleware de exceções** está em [MIDDLEWARE_EXCECOES.md](MIDDLEWARE_EXCECOES.md).

---

## 1. Autenticação e pipeline

### 1.1 UseAuthentication no pipeline

- **Problema:** O JWT não era validado e endpoints com `[Authorize]` não funcionavam corretamente.
- **Alteração:** Inclusão de `app.UseAuthentication()` no pipeline, **antes** de `app.UseAuthorization()`, em `DbMercado.Api/Program.cs`.
- **Arquivo:** `DbMercado.Api/Program.cs`.

### 1.2 Expiração do JWT e LoginResponse

- **Problema:** A expiração do token e da resposta de login estava fixa em 1 hora no código, ignorando a configuração.
- **Alteração:**
  - Em `AuthRepository` (Infrastructure): o token JWT passa a usar `_jwtSettings.ExpirationHours` ao definir `Expires`.
  - Em `AuthController` (Api): o `LoginResponse` passa a usar `jwtSettings.Value.ExpirationHours` para calcular a data de expiração retornada ao cliente.
- **Arquivos:** `DbMercado.Infrastructure/Services/Autenticacao/AuthRepository.cs`, `DbMercado.Api/Controllers/Autenticacao/AuthController.cs`.

### 1.3 Políticas de autorização

- **Problema:** O `TestController` usava políticas `CanRead`, `CanViewLogs` e `CanManageMenus` que não estavam registradas, resultando em 403.
- **Alteração:** Registro das políticas em `Program.cs`:
  - **CanRead:** exige a claim `NivelAcesso`.
  - **CanViewLogs** e **CanManageMenus:** exigem a claim `NivelAcesso` com valor de Administrador (100).
- **Arquivo:** `DbMercado.Api/Program.cs`.

---

## 2. Configuração e infraestrutura

### 2.1 Connection string no appsettings

- **Problema:** Connection string e senha do banco estavam hardcoded em `ApplicationSettings.cs` (Domain), com risco de segurança.
- **Alteração:**
  - Inclusão da chave `ConnectionStrings:DefaultConnection` em `DbMercado.Api/appsettings.json`.
  - Leitura da connection string em `Program.cs` com `GetConnectionString("DefaultConnection")` e registro do `DbContext` com `UseSqlServer(connectionString)`.
  - Remoção da região **Database** (connection strings) de `ApplicationSettings.cs`.
  - `AppDbContext` deixou de usar `ApplicationSettings.DataBase.ConnectionString`; o contexto é configurado apenas via opções em `Program.cs`.
- **Arquivos:** `DbMercado.Api/appsettings.json`, `DbMercado.Api/Program.cs`, `DbMercado.Domain/ApplicationSettings.cs`, `DbMercado.Infrastructure/Data/AppDbContext.cs`.

### 2.2 Centralização de opções do DbContext

- **Problema:** Duplicidade de configuração entre `Program.cs` (UseSqlServer + ConfigureWarnings) e `AppDbContext.OnConfiguring`.
- **Alteração:**
  - Em `Program.cs`: apenas `UseSqlServer(connectionString)` na configuração do `DbContext`.
  - Em `AppDbContext`: apenas `ConfigureWarnings` (RowLimitingOperationWithoutOrderByWarning) em `OnConfiguring`, como único lugar das opções de comportamento do EF.
- **Arquivos:** `DbMercado.Api/Program.cs`, `DbMercado.Infrastructure/Data/AppDbContext.cs`.

### 2.3 Solution na raiz do repositório

- **Problema:** Abrir o backend só pela solution dentro de `DbMercado.Api` gerava duplicidade e confusão em repositório multi-repo.
- **Alteração:** Manter apenas `DbMercado.sln` na raiz do repositório backend, referenciando todos os projetos (`DbMercado.Api/`, `DbMercado.Application/`, etc.). A solution duplicada em `DbMercado.Api/` foi removida.
- **Arquivo:** `DbMercado.sln` (raiz).

### 2.4 Abertura do Swagger ao iniciar a API

- **Problema:** O navegador não abria automaticamente ao subir a API.
- **Alteração:** Em `DbMercado.Api/Properties/launchSettings.json`: `launchBrowser: true` e `launchUrl: "swagger"` nos perfis `http` e `https`.
- **Arquivo:** `DbMercado.Api/Properties/launchSettings.json`.

---

## 3. Tratamento global de exceções e logging

### 3.1 Middleware de exceções (ErrorHandlingMiddleware)

- **Objetivo:** Tratamento global de exceções e respostas padronizadas em ProblemDetails.
- **Alteração:**
  - Criação de `ErrorHandlingMiddleware` em `DbMercado.Api/Middlewares/ErrorHandlingMiddleware.cs`.
  - Criação de scope de log por requisição (TraceId, UserId, UserName, Path, Method, Ip, UserAgent, RouteVals).
  - Captura de exceções, mapeamento para HTTP (401, 404, 409, 422, 499, 500) e escrita de `application/problem+json`.
  - Registro em `Program.cs`: `app.UseMiddleware<ErrorHandlingMiddleware>()` antes de autenticação e autorização.
- **Documentação detalhada:** [docs/MIDDLEWARE_EXCECOES.md](MIDDLEWARE_EXCECOES.md).
- **Arquivos:** `DbMercado.Api/Middlewares/ErrorHandlingMiddleware.cs`, `DbMercado.Api/Program.cs`.

### 3.2 AddProblemDetails

- **Alteração:** Inclusão de `builder.Services.AddProblemDetails()` em `Program.cs` para suporte a respostas ProblemDetails.
- **Arquivo:** `DbMercado.Api/Program.cs`.

---

## 4. Persistência de logs no banco

### 4.1 Entidade e tabela de log

- **Alteração:**
  - Criação da entidade `AppLogEntry` em `Infrastructure/Data/Entities/AppLogEntry.cs` (Category, Level, Message, Exception, ErrorCode, TraceId, UserId, UserName, Path, Method, Ip, UserAgent, CreatedAt).
  - Inclusão de `DbSet<AppLogEntry>` e mapeamento da tabela `AppLog` em `AppDbContext`.
  - Migration `AddAppLogTable` para criação da tabela `AppLog`.
- **Arquivos:** `DbMercado.Infrastructure/Data/Entities/AppLogEntry.cs`, `DbMercado.Infrastructure/Data/AppDbContext.cs`, `DbMercado.Infrastructure/Migrations/20260222120000_AddAppLogTable.cs` (e Designer + snapshot).

### 4.2 DatabaseLoggerProvider e persistência

- **Alteração:**
  - `DatabaseLoggerProvider` passou a receber `IServiceScopeFactory` no construtor.
  - Em cada chamada a `Log()`, o provider abre um novo scope, resolve `AppDbContext`, monta um `AppLogEntry` com dados do scope (TraceId, UserId, Path, etc.) e do state (Message, ErrorCode, Exception), trunca campos conforme os tamanhos da tabela e chama `Add` + `SaveChanges`.
  - Falhas ao gravar são ignoradas (try/catch) para não derrubar a aplicação.
- **Arquivos:** `DbMercado.Infrastructure/Logging/DatabaseLoggerProvider.cs`.

### 4.3 Registro do provider e uso no pipeline

- **Alteração:**
  - Registro do provider em `Program.cs`: `builder.Services.AddSingleton<DatabaseLoggerProvider>()` na região de injeção de dependência de serviços (junto com `IAuthService`, etc.).
  - Extensão `app.UseDatabaseLogger()` em `DbMercado.Api/Extensions/LoggingAppExtensions.cs`: após `Build()`, obtém o `ILoggerFactory`, faz cast para `LoggerFactory` e adiciona o `DatabaseLoggerProvider` com `AddProvider`.
  - Chamada a `app.UseDatabaseLogger()` logo após `var app = builder.Build()`.
- **Arquivos:** `DbMercado.Api/Program.cs`, `DbMercado.Api/Extensions/LoggingAppExtensions.cs`.

### 4.4 Extensão AddDatabaseLogger

- O método de extensão `AddDatabaseLogger()` em `DbMercado.Infrastructure/Extensions/LoggingExtensions.cs` **não é mais utilizado**; o registro do `DatabaseLoggerProvider` foi movido para `Program.cs`. O arquivo de extensão pode ser removido ou mantido para uso futuro.

---

## 5. Resumo de arquivos alterados ou criados

| Arquivo | Tipo de alteração |
|--------|--------------------|
| `DbMercado.Api/Program.cs` | Várias: auth, connection string, policies, ProblemDetails, middleware, registro do DatabaseLoggerProvider, UseDatabaseLogger |
| `DbMercado.Api/Controllers/Autenticacao/AuthController.cs` | Expiração do login baseada em JwtSettings |
| `DbMercado.Api/Middlewares/ErrorHandlingMiddleware.cs` | **Criado** |
| `DbMercado.Api/Extensions/LoggingAppExtensions.cs` | **Criado** |
| `DbMercado.Api/Properties/launchSettings.json` | launchBrowser e launchUrl |
| `DbMercado.Api/appsettings.json` | ConnectionStrings e formatação |
| `DbMercado.Domain/ApplicationSettings.cs` | Remoção da região Database |
| `DbMercado.Infrastructure/Data/AppDbContext.cs` | DbSet AppLogEntries, configuração AppLog, apenas ConfigureWarnings em OnConfiguring |
| `DbMercado.Infrastructure/Data/Entities/AppLogEntry.cs` | **Criado** |
| `DbMercado.Infrastructure/Services/Autenticacao/AuthRepository.cs` | Expiração do token com JwtSettings.ExpirationHours |
| `DbMercado.Infrastructure/Logging/DatabaseLoggerProvider.cs` | IServiceScopeFactory e persistência em AppLog |
| `DbMercado.Infrastructure/Extensions/LoggingExtensions.cs` | AddDatabaseLogger apenas registra singleton (não usado atualmente) |
| `DbMercado.Infrastructure/Migrations/20260222120000_AddAppLogTable.cs` | **Criado** |
| `DbMercado.Infrastructure/Migrations/20260222120000_AddAppLogTable.Designer.cs` | **Criado** |
| `DbMercado.Infrastructure/Migrations/AppDbContextModelSnapshot.cs` | Entidade AppLogEntry |
| `DbMercado.sln` (raiz) | **Criado** |
| `README.md` | Atualizado com instruções e referências |
| `docs/MIDDLEWARE_EXCECOES.md` | **Criado** – comportamento detalhado do middleware de exceções |
| `docs/ALTERACOES_DO_PROJETO.md` | **Criado** – este arquivo |

---

## 6. Aplicar a migration do log

Para criar a tabela `AppLog` no banco:

```bash
dotnet ef database update --project DbMercado.Infrastructure --startup-project DbMercado.Api
```

Execute na raiz do repositório, com a connection string já configurada (appsettings ou User Secrets).
