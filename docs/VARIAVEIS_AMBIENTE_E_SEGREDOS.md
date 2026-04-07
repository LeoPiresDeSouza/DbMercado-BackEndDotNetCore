# Variáveis de ambiente e segredos (API)

Documentação de configuração **sensível** da `DbMercado.Api`. **Não** versionar valores reais de produção em `appsettings.json` commitado; use **User Secrets** (desenvolvimento), **variáveis de ambiente** ou um cofre (Azure Key Vault, etc.) em produção.

A precedência padrão do .NET aplica-se (`appsettings` → `appsettings.{Environment}.json` → User Secrets → variáveis de ambiente → argumentos de linha de comando). Detalhes da **connection string** estão em [STRING-CONEXAO.md](./STRING-CONEXAO.md).

---

## ConnectionStrings

| Chave | Descrição |
|--------|-----------|
| `ConnectionStrings__DefaultConnection` | SQL Server (formato ADO.NET). Também aceita seção JSON `ConnectionStrings:DefaultConnection`. |

Exemplo (variável de ambiente):

```text
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;
```

---

## JWT (`JwtSettings`)

| Chave / caminho | Descrição |
|------------------|-----------|
| `JwtSettings__Secret` | Chave simétrica de assinatura (não versionar). |
| `JwtSettings__Issuer` | Emissor do token. |
| `JwtSettings__Audience` | Audiência. |
| `JwtSettings__ExpirationHours` | Validade do access token em horas. |

---

## Chat multilíngue — criptografia em repouso

| Chave | Descrição |
|--------|-----------|
| `CHAT_ENCRYPTION_KEY` | Chave **AES-256** codificada em **Base64** que decodifica para **exatamente 32 bytes**. Usada pelo `AesEncryptionService` para campos de texto de mensagens (`ContentOriginal`, `TranslatedText`). Ausência ou valor inválido impede a subida da aplicação. |

Geração de chave (32 bytes aleatórios → Base64), exemplo PowerShell:

```powershell
$b = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
[Convert]::ToBase64String($b)
```

Ou, com OpenSSL: `openssl rand -base64 32` (confirme que a decodificação resulta em exatamente 32 bytes).

---

## OpenRouter (tradução IA)

| Chave / caminho | Descrição |
|------------------|-----------|
| `OpenRouter__ApiKey` | Bearer token da API [OpenRouter](https://openrouter.ai/) (modelo configurado no código: DeepSeek via completions). |
| Seção `OpenRouter` em JSON | Alternativa: `"OpenRouter": { "ApiKey": "" }` preenchida localmente ou via User Secrets. |

**Nunca** commitar a chave real. Em desenvolvimento, prefira:

```bash
dotnet user-secrets set "OpenRouter:ApiKey" "sua-chave" --project DbMercado.Api
```

---

## Outras referências

- Permissões e seed do módulo administrativo / chat: [MODULO_ADMINISTRATIVO.md](./MODULO_ADMINISTRATIVO.md)
- Arquitetura do chat: `docs/CHAT_MODULE.md` (repositório pai)

---

## Swagger

Com a API em execução, a UI OpenAPI fica em `/swagger` (perfil típico de desenvolvimento). Os endpoints de chat aparecem agrupados na tag **Chat multilíngue**, com respostas e comentários XML alinhados aos contratos acima.
