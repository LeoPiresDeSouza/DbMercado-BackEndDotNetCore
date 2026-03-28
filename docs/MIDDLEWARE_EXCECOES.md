# Middleware de Tratamento de Exceções (ErrorHandlingMiddleware)

**Localização:** `DbMercado.Api/Middlewares/ErrorHandlingMiddleware.cs`

Este documento descreve em detalhe o comportamento do middleware global de exceções da API.

---

## 1. Objetivo

- **Capturar** todas as exceções não tratadas que ocorrem durante o processamento da requisição (incluindo em controllers, serviços e repositórios).
- **Padronizar** a resposta de erro em formato **RFC 7807 Problem Details** (`application/problem+json`).
- **Enriquecer o log** com um scope por requisição (TraceId, UserId, Path, etc.), para que qualquer log gerado dentro do pipeline compartilhe esse contexto.

Os controllers **não precisam** fazer try/catch: as exceções sobem até o middleware, que as converte em resposta HTTP e log.

---

## 2. Ordem no pipeline

O middleware deve ser registrado **antes** de `UseAuthentication()` e `UseAuthorization()`, e **depois** do seeder do banco (se houver). Assim:

- Qualquer exceção lançada **antes** da autenticação (ou durante ela) é tratada pelo middleware.
- O **scope de log** é criado no início do processamento, então todos os logs da requisição (incluindo os gerados ao tratar a exceção) já têm TraceId, Path, UserId, etc.

Ordem típica no `Program.cs`:

```text
app.DataBaseSeeder();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## 3. Fluxo de execução

### 3.1 Início da requisição

1. O middleware obtém dados do **HttpContext** e do **User** (quando já autenticado):
   - **TraceId**: `Activity.Current?.TraceId` ou `context.TraceIdentifier`
   - **UserId**: claim `NameIdentifier` ou `sub` (JWT)
   - **UserName**: `context.User.Identity?.Name`
   - **Path**, **Method**, **RouteVals**, **Ip**, **UserAgent**

2. Cria um **scope de log** com esse dicionário e chama `await _next(context)` dentro desse scope.

Enquanto a requisição estiver sendo processada, qualquer `ILogger` usado na mesma cadeia terá acesso a esse scope (por exemplo, no `DatabaseLoggerProvider` ou no Console).

### 3.2 Quando não há exceção

- `_next(context)` termina normalmente.
- A resposta já foi escrita pelos controllers/middlewares posteriores.
- O scope é encerrado ao sair do `using (_logger.BeginScope(scope))`.

### 3.3 Quando ocorre exceção

1. **OperationCanceledException** com `context.RequestAborted.IsCancellationRequested`:
   - Considerado “cliente fechou a conexão” (ex.: usuário saiu da página).
   - Log: `LogInformation("Requisição cancelada pelo cliente.")`.
   - Resposta: status **499 Client Closed Request** (sem corpo ProblemDetails).

2. **Qualquer outra exceção**:
   - Se **já começou a enviar a resposta** (`context.Response.HasStarted`):
     - Não tenta escrever no corpo; loga a exceção e **relança** (`throw`), para o servidor tratar.
   - Caso contrário:
     - Chama `HandleExceptionAsync(context, ex)`.

---

## 4. HandleExceptionAsync e mapeamento de exceções

`HandleExceptionAsync`:

1. Chama **MapException(ex)** para obter: `status`, `title`, `type`, `errorCode`, `detail`, `isBusiness`.
2. **Loga** a exceção:
   - Se `isBusiness`: `LogWarning` com mensagem e `ErrorCode`.
   - Senão: `LogError` (erro inesperado).
3. Monta um **ProblemDetails** com:
   - `Status`, `Title`, `Type`, `Detail`, `Instance` (path da requisição).
   - Em **Extensions**: `errorCode` e `traceId`.
4. Define:
   - `context.Response.StatusCode = status`
   - `context.Response.ContentType = "application/problem+json"`
5. Serializa o ProblemDetails com **System.Text.Json** e escreve no corpo da resposta.

### 4.1 Mapeamento (MapException)

| Exceção | HTTP Status | Title | errorCode | Observação |
|--------|-------------|--------|-----------|------------|
| **BusinessException** (exceto DuplicateEntityException) | 422 Unprocessable Entity | Violação de regra de negócio | `bex.ErrorCode` | Regras de negócio (ex.: “já existe um menu com esse nome”). |
| **DuplicateEntityException** | 409 Conflict | Entidade duplicada | `dex.ErrorCode` | Herda de BusinessException; tratada antes para retornar 409. |
| **EntityNotFoundException** | 404 Not Found | Recurso não encontrado | `NOT_FOUND` | Recurso não existe (ex.: id inexistente). |
| **UnauthorizedAccessException** | 401 Unauthorized | Não autorizado | `UNAUTHORIZED` | Mensagem fixa de permissão. |
| **Demais exceções** | 500 Internal Server Error | Erro interno | `INTERNAL_SERVER_ERROR` | Mensagem genérica; detalhes só no log. |

As exceções de domínio estão em `DbMercado.Domain.Exceptions` (BusinessException, EntityNotFoundException, DuplicateEntityException).

### 4.2 Exemplo de resposta (422)

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Violação de regra de negócio",
  "status": 422,
  "detail": "Já existe um menu com esse nome.",
  "instance": "/api/menu",
  "errorCode": "MENU_DUPLICADO",
  "traceId": "00-abc123..."
}
```

O cliente pode usar `errorCode` e `traceId` para suporte e correlação com logs.

---

## 5. Uso nas camadas de aplicação

- **Controllers:** apenas chamam serviços e retornam resultados; **não** capturam exceções.
- **Application (AppServices):** validam regras de negócio e lançam `BusinessException` ou `DuplicateEntityException` quando aplicável; para “não encontrado”, podem lançar `EntityNotFoundException`.
- **Infrastructure (repositórios):** podem lançar `EntityNotFoundException` quando um recurso não existe.

O middleware garante que todas essas exceções se tornem respostas HTTP padronizadas e que o log tenha o contexto da requisição (incluindo quando a exceção é logada dentro do próprio middleware).
