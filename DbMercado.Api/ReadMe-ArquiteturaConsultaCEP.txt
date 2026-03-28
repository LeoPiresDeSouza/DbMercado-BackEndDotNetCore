src
 ├── Domain
 │
 ├── Application
 │   ├── Services
 │   │      CepService
 │   │
 │   ├── Interfaces
 │   │      ICepService
 │   │      ICepProvider
 │   │
 │   └── DTOs
 │          EnderecoDto
 │
 ├── Infrastructure
 │   ├── Providers
 │   │      ViaCepProvider
 │   │      BrasilApiProvider
 │   │      FallbackCepProvider
 │   │
 │   ├── Http
 │   │      HttpPolicies
 │   │
 │   └── Utils
 │          CepNormalizer
 │
 └── API
     ├── Controllers
     │      CepController
     │
     └── Configuration
            HttpClientConfig


Arquitetura de Consulta de CEP
Objetivo

O módulo de consulta de CEP tem como objetivo fornecer resolução de endereço a partir de CEP informado pelo usuário, utilizando múltiplas APIs externas com mecanismos de resiliência para garantir:

alta disponibilidade

tolerância a falhas externas

baixa latência

proteção contra sobrecarga

A arquitetura implementa:

Cache em memória

Normalização de CEP

Múltiplos provedores de CEP

Fallback automático entre provedores

Retry exponencial

Circuit Breaker

Rate Limiting




Fluxo geral da requisição

Quando o cliente consulta um CEP:

GET /api/cep/{cep}

Fluxo interno da aplicação:

CepController
      │
      ▼
CepService
      │
      ▼
Cache lookup
      │
      ├── HIT → retorna imediatamente
      │
      └── MISS
              │
              ▼
      FallbackCepProvider
              │
              ├── ViaCepProvider
              │        │
              │        ▼
              │    HttpClient + Policies
              │
              └── BrasilApiProvider
                       │
                       ▼
                  HttpClient + Policies




Camadas na arquitetura DDD
Domain

Nesta funcionalidade não existem elementos de domínio.

A consulta de CEP é uma integração externa, não representa uma regra de negócio central da aplicação.

Portanto nenhum componente deste módulo reside na camada Domain.

Application

Responsável por orquestrar o caso de uso.

Componentes:

Application
 ├── Interfaces
 │      ICepService
 │      ICepProvider
 │
 ├── Services
 │      CepService
 │
 └── DTOs
        EnderecoDto
Infrastructure

Responsável por integração com sistemas externos.

Infrastructure
 ├── Providers
 │      ViaCepProvider
 │      BrasilApiProvider
 │      FallbackCepProvider
 │
 ├── Http
 │      HttpPolicies
 │
 └── Utils
        CepNormalizer
API

Responsável pela exposição HTTP.

API
 ├── Controllers
 │      CepController
 │
 └── Configuration
        HttpClient configuration
        Rate limiting



Componentes detalhados
CepController

Responsabilidade:

expor endpoint HTTP

validar parâmetros básicos

delegar execução ao serviço de aplicação

Exemplo de endpoint:

GET /api/cep/{cep}

O controller não contém lógica de negócio.

Ele apenas chama:

CepService.ConsultarCepAsync()
CepService

Responsabilidade:

implementar o caso de uso da aplicação

gerenciar cache

delegar consulta aos providers

Fluxo interno:

normalizar CEP

validar formato

consultar cache

chamar provider

mapear resposta para DTO

armazenar resultado em cache

Pseudo fluxo:

normalize CEP
     │
cache lookup
     │
     ├── encontrado → retorna
     │
     └── não encontrado
            │
            ▼
      provider.BuscarCep()
            │
            ▼
        resultado
            │
            ▼
        salvar cache
CepNormalizer

Responsabilidade:

padronizar CEP antes de qualquer consulta externa.

Exemplo de entradas possíveis:

50710-140
50710140
50.710-140

Resultado:

50710140

Implementação remove todos os caracteres não numéricos.

Isso garante compatibilidade com APIs externas.

ICepProvider

Interface que define o contrato de consulta de CEP.

Task<CepResponse?> BuscarCepAsync(string cep)

Essa abstração permite trocar facilmente a fonte de dados.

Qualquer nova API pode ser integrada apenas implementando esta interface.

Providers de CEP

Cada provider é responsável por integrar com uma API específica.

Providers implementados:

ViaCepProvider

BrasilApiProvider

ViaCepProvider

Responsabilidade:

chamar API externa do serviço ViaCEP

desserializar resposta

mapear para modelo interno

Fluxo:

HttpClient
     │
     ▼
GET /ws/{cep}/json
     │
     ▼
JSON Response
     │
     ▼
Mapear para CepResponse
BrasilApiProvider

Responsabilidade:

chamar API externa BrasilAPI

converter resposta para modelo interno

Fluxo semelhante ao provider anterior.

HttpPolicies

Classe responsável por definir políticas de resiliência HTTP utilizando Polly.

As políticas são aplicadas automaticamente ao HttpClient.

Retry Policy

Objetivo:

repetir chamadas quando ocorre falha transitória.

Exemplo:

tentativa 1 → falha
200 ms
tentativa 2 → falha
400 ms
tentativa 3 → sucesso

Estratégia utilizada:

exponential backoff

Isso evita sobrecarga na API externa.

Circuit Breaker Policy

Objetivo:

evitar chamadas repetidas para serviços instáveis.

Configuração:

5 falhas consecutivas
→ circuito abre
→ chamadas bloqueadas por 30 segundos

Estados possíveis:

Closed
→ funcionamento normal

Open
→ chamadas bloqueadas

Half-open
→ tentativa de teste após período de bloqueio

Se a chamada de teste funcionar:

circuito fecha
FallbackCepProvider

Este componente é o responsável pelo mecanismo de fallback entre APIs.

Ele recebe uma lista de providers disponíveis através de injeção de dependência.

Exemplo interno:

_providers =
[
    ViaCepProvider,
    BrasilApiProvider
]
Funcionamento do fallback

O fallback ocorre por tentativa sequencial.

Pseudo código simplificado:

para cada provider na lista
    resultado = provider.BuscarCep()

    se resultado != null
        retornar resultado

se nenhum provider funcionar
    retornar null

Fluxo real:

ViaCEP
   │
   ├── sucesso → retorna resultado
   │
   └── falha
          │
          ▼
BrasilAPI
   │
   ├── sucesso → retorna resultado
   │
   └── falha
          │
          ▼
retorna null
Como o fallback é ativado

O fallback não é um mecanismo automático da biblioteca.

Ele ocorre explicitamente pela lógica de controle dentro do FallbackCepProvider.

O fallback é ativado quando:

provider retorna null
OU
provider lança exceção
OU
HttpClient retorna falha

Quando isso ocorre, o loop continua para o próximo provider.

HttpClientFactory

O .NET fornece o componente:

IHttpClientFactory

Ele resolve problemas clássicos de uso incorreto de HttpClient:

esgotamento de sockets

má reutilização de conexões

problemas de DNS caching

O factory mantém internamente um pool de HttpClients reutilizáveis.

Cada provider obtém um client configurado:

factory.CreateClient("ViaCep")
Rate Limiting

Protege a API contra excesso de requisições.

Exemplo de configuração:

PermitLimit = 20
Window = 10 segundos

Significado:

máximo de 20 requisições a cada 10 segundos

Quando o limite é excedido:

HTTP 429
Too Many Requests

Isso evita:

abuso da API

loops de frontend

ataques simples

Cache

A consulta de CEP utiliza cache em memória.

Fluxo:

consulta CEP
      │
      ▼
cache lookup
      │
      ├── encontrado
      │       │
      │       ▼
      │   retorna imediatamente
      │
      └── não encontrado
              │
              ▼
      consulta API externa
              │
              ▼
         salvar cache

Tempo de expiração sugerido:

30 dias

CEP raramente muda.

Benefícios da arquitetura

Esta abordagem proporciona:

tolerância a falhas externas

latência reduzida

baixo acoplamento

facilidade de manutenção

fácil adição de novos providers

Exemplo de extensão futura:

CorreiosProvider
GoogleMapsProvider
Base local de CEP

Sem alteração no serviço de aplicação.

Se quiser, posso também te fornecer uma versão ainda mais profissional dessa documentação, incluindo:

diagramas de sequência

diagramas de componentes

estrutura completa de pastas do projeto

exemplo de testes unitários para cada provider

Esse tipo de documentação costuma ficar excelente em README de repositório ou pasta /docs/architecture do projeto.
