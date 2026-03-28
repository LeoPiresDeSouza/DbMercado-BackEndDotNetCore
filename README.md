# DbMercado - Backend API Documentation

Este documento fornece informações essenciais para a integração do frontend React com a API backend em .NET Core.

## 1. Visão Geral da API

A API `DbMercado` é construída com .NET Core 9.0 e segue princípios de Domain-Driven Design (DDD), Clean Architecture e CQRS. Ela provê endpoints para gerenciamento de módulos, usuários e outras funcionalidades relacionadas ao sistema.

## 2. Autenticação

A autenticação na API é realizada através de JSON Web Tokens (JWT).

### Fluxo de Autenticação:

1.  Um cliente (frontend React) envia uma requisição `POST` para o endpoint `/api/Auth/login` com as credenciais do usuário (e-mail e senha) no corpo da requisição.
2.  Se as credenciais forem válidas, a API retorna um JWT (token) e a data/hora de expiração.
3.  O frontend deve armazenar esse token de forma segura (ex: `localStorage`, `sessionStorage` ou `httpOnly cookies`).
4.  Para acessar endpoints protegidos, o frontend deve incluir o JWT no cabeçalho `Authorization` de todas as requisições subsequentes, no formato: `Authorization: Bearer <seu_token_jwt>`.

**Exemplo de endpoint de login:**
*   **URL:** `/api/Auth/login`
*   **Método:** `POST`
*   **Corpo da Requisição (JSON):**
    ```json
    {
      "email": "usuario@exemplo.com",
      "password": "sua_senha"
    }
    ```
*   **Resposta de Sucesso (JSON):**
    ```json
    {
      "token": "seu_jwt_aqui",
      "expiration": "2026-03-23T23:59:59Z"
    }
    ```

## 3. Autorização (Políticas de Acesso)

A API utiliza políticas de autorização baseadas em claims para controlar o acesso a endpoints específicos. O token JWT deve conter as claims necessárias para o acesso.

As políticas de autorização atualmente definidas são:

*   **`CanRead`**: Requer a claim `"NivelAcesso"`. Permite acesso geral de leitura.
*   **`CanViewLogs`**: Requer a claim `"NivelAcesso"` com o valor correspondente ao `Administrador`.
*   **`CanManageMenus`**: Requer a claim `"NivelAcesso"` com o valor correspondente ao `Administrador`.

**Exemplo de endpoint que requer autorização:**
*   **URL:** `/api/Test/authenticated`
*   **Método:** `GET`
*   **Cabeçalho da Requisição:** `Authorization: Bearer <seu_token_jwt>`

## 4. Endpoints Principais

A seguir, uma lista de alguns endpoints importantes:

*   **Autenticação:**
    *   `POST /api/Auth/login`: Autentica um usuário e retorna um JWT.
*   **Módulos de Usuário:**
    *   `GET /api/Modulo/modulosUsuario`: Retorna a estrutura de acesso de módulos, funcionalidades e permissões para um usuário autenticado.

## 5. Configuração do Frontend

Ao desenvolver o frontend React, é importante configurar a URL base da API. Recomenda-se o uso de variáveis de ambiente para gerenciar essas configurações.

**URL Base da API (Exemplo para desenvolvimento local):**
`http://localhost:5000` (ou a porta configurada no `launchSettings.json` do projeto DbMercado.Api)

**Exemplo (arquivo .env no projeto React):**
```
REACT_APP_API_BASE_URL=http://localhost:5000
```
Certifique-se de ajustar este valor para o ambiente de produção.

## 6. Tratamento de Erros

A API retorna erros padronizados no formato [Problem Details (RFC 7807)](https://datatracker.ietf.org/doc/html/rfc7807). Isso significa que, em caso de erro, a resposta HTTP conterá um JSON com detalhes sobre o problema, incluindo:

*   `status`: Código de status HTTP.
*   `title`: Título conciso do erro.
*   `type`: URI que identifica o tipo de erro.
*   `detail`: Detalhes específicos sobre a ocorrência do erro.
*   `instance`: O URI do recurso ao qual o problema se aplica.
*   `errorCode`: Código de erro interno da aplicação para referência.
*   `traceId`: ID de rastreamento para depuração.

**Exemplo de Resposta de Erro (JSON):**
```json
{
  "status": 422,
  "title": "Violação de regra de negócio",
  "type": "https://httpstatuses.com/422",
  "detail": "O campo 'Email' deve ser um endereço de e-mail válido.",
  "instance": "/api/Auth/login",
  "errorCode": "INVALID_EMAIL",
  "traceId": "00-8e1a7b6c5d4e3f2a1b0c9d8e7f6a5b4c-9f8e7d6c5b4a3f2e-00"
}
```
