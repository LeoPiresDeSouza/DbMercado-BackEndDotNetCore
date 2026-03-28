# Convenções — Integrações externas (Backend .NET)

Este documento fixa como o projeto trata **Integrações** (marketplaces, bancos, gateways de pagamento, webhooks, serviços de IA via HTTP, etc.) sem misturar contextos de domínio nem colocar regras de negócio na camada de infraestrutura técnica.

---

## 1. Princípio

- **Integração não é um bounded context de domínio.** É capacidade de **infraestrutura** que *serve* a Pedidos, Financeiro, Importação, etc.
- **Contratos orientados ao consumidor:** interfaces abstratas ficam no **Domain** (ou, quando for apenas orquestração de aplicação sem conceito de domínio, na **Application**) do **contexto que consome** a integração — nunca num namespace genérico “Integrações” no domínio.

Exemplos de nomes por contexto:

| Contexto consumidor | Exemplo de contrato (interface) | Implementação |
|---------------------|----------------------------------|---------------|
| Financeiro | `IPaymentGateway`, `IBankReconciliationClient` | `Infrastructure/Integracoes/Bancos/` ou `.../Pagamentos/` |
| Pedidos / Marketplace | `IMarketplaceOrderClient`, `IInventorySyncClient` | `Infrastructure/Integracoes/Marketplace/` |
| Plataforma | `IWebhookSignatureValidator` (se transversal) | `Infrastructure/Integracoes/Webhooks/` ou `Shared/HTTP` |

---

## 2. Estrutura de pastas na Infrastructure

```
DbMercado.Infrastructure/
  Integracoes/
    Marketplace/     # clientes específicos de marketplaces (por vendedor/API)
    Bancos/          # Open Finance, boletos, extratos, quando aplicável
    Webhooks/        # recebimento e validação de callbacks externos
    Ia/              # cliente HTTP para o backend Python (OCR, enriquecimento), sem lógica de ML no .NET
```

- **Adapters:** classes concretas que implementam as interfaces do Domain/Application e usam `HttpClient`, SDKs oficiais ou filas (futuro).
- **DTOs de integração:** modelos *anti corrupção* (formato da API externa) ficam junto do adapter ou em subpasta `Models`, **não** como entidades de domínio.

---

## 3. Registro em DI (`Program.cs`)

- Usar `IHttpClientFactory` com clientes nomeados (`AddHttpClient("MercadoLivre", ...)`) e Polly quando necessário (como já feito para CEP).
- Registrar **implementação → interface** com `AddScoped` ou `AddHttpClient<T>` conforme ciclo de vida desejado.
- **Segredos** apenas via configuração (`appsettings`, User Secrets, variáveis de ambiente), nunca hardcoded.

---

## 4. O que não fazer

- Colocar regra de negócio (ex.: cálculo de comissão, elegibilidade de estoque) dentro de classes de integração; essas regras permanecem no **Domain** / **Application** do contexto dono.
- Referenciar entidades EF (`*Entity`) diretamente nos DTOs de resposta de API externa.
- Fazer integrações síncronas longas no meio de request HTTP sem timeout/circuit breaker; preferir fila + job (Quartz) para sincronização em lote, quando o documento estratégico assim exigir.

---

## 5. IA (backend Python)

- O .NET mantém apenas **orquestração**: chamadas REST, retries, persistência do resultado em tabelas do contexto adequado (ex.: Importação).
- Modelos e inferência ficam no **serviço Python**; evoluir para fila (RabbitMQ/Kafka) quando o volume ou a confiabilidade exigirem.

---

## 6. Evolução

Ao adicionar um novo provedor:

1. Definir interface no Domain (ou Application) do contexto consumidor.
2. Implementar adapter em `Infrastructure/Integracoes/<área>/`.
3. Registrar no DI e cobrir com testes de integração (opcional) ou testes com `HttpMessageHandler` fake.

Esta convenção complementa o documento estratégico e o plano de bounded contexts do repositório.
