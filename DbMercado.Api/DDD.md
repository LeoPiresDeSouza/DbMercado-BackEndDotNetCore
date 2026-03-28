

Com certeza! Como estamos em uma interface de chat, a melhor forma de eu te entregar isso é em formato **Markdown (`.md`)**. Esse é o formato padrão utilizado na indústria para documentação em repositórios (como o `README.md`).

Você pode copiar o conteúdo abaixo, criar um arquivo chamado `ARCHITECTURE_GUIDELINES.md` (ou `PADROES_ARQUITETURA.md`) na raiz do seu projeto ou em uma pasta `/docs`, e colar lá dentro. O GitHub, Azure DevOps e o próprio Visual Studio/VS Code renderizam esse formato perfeitamente.

Aqui está o seu documento técnico:

---

# Guia de Referência Arquitetural: DDD, Clean Architecture e CQRS
**Stack:** .NET 9, C# | **Frontend:** React

Este documento estabelece as diretrizes arquiteturais para o backend do projeto, focado em resolver a complexidade de mapeamentos, divisão de responsabilidades, integração com serviços externos e estruturação das camadas.

---

## 1. O Padrão CQRS (Separação de Leitura e Escrita)
O maior anti-padrão em DDD é usar Repositórios e Entidades de Domínio para montar telas complexas, gerando um excesso de Mappers ("mapper para lá e para cá"). Para resolver isso, aplicamos **CQRS**.

### 1.1. Fluxo de Escrita (Commands: Insert, Update, Delete)
Apenas na escrita nós utilizamos o **Domain Driven Design (DDD) puro**.
* **Regra:** Repositórios pertencem ao Domínio. Eles recebem e retornam apenas **Entidades** ou **Value Objects**. Eles não conhecem DTOs.
* **Fluxo:** `Controller` ➔ `AppService` (ou MediatR) ➔ `Entity` ➔ `Repository` (Infra salva no banco).
* **Mappers:** Ocorrem apenas na Application para converter o `RequestDTO` em uma `Entidade`.

### 1.2. Fluxo de Leitura (Queries: Buscas, Filtros, Listagens)
Para consultas complexas, **ignoramos o Domínio e os Repositórios**.
* **Regra:** O banco de dados é tratado apenas como um depósito de dados. Não há validação de regra de negócio em um simples `SELECT`.
* **Como fazer:**
  1. A camada de **Application** define o `FiltroDTO`, o `ResponseDTO` e a interface da query (ex: `IUsuarioQueries`).
  2. A camada de **Infrastructure** implementa a interface, roda a query otimizada (ex: Dapper ou EF Core com `.Select()`) com todos os `JOINs` necessários.
  3. O resultado da query já é projetado **diretamente no `ResponseDTO`**.
* **Vantagem:** Zero necessidade de mapear banco ➔ Entidade ➔ DTO. O dado vai do banco direto para o DTO de resposta.

---

## 2. Integração com APIs Externas (Correios, ViaCEP, etc.)
A camada de Infraestrutura **jamais dita o formato de retorno**. Aplicamos o **Princípio da Inversão de Dependência (DIP)**. O "De-Para" (Mapper do JSON externo para o nosso modelo) fica escondido dentro da Infraestrutura.

O local onde a Interface será declarada depende do seu propósito:

### Cenário A: Serviço de Aplicação (Apoio à UI)
Se a consulta externa serve apenas para facilitar o preenchimento de telas no frontend e não afeta o cálculo das regras de negócio:
* **Interface e DTO:** Ficam na camada de **Application**.
* **Implementação:** A **Infrastructure** consome a API externa, converte o JSON sujo no `DTO` limpo da Application e o devolve.

### Cenário B: Serviço de Domínio (Regra de Negócio)
Se a consulta externa traz um dado vital para uma tomada de decisão do negócio (ex: O Estado (UF) define a alíquota de imposto da Entidade `Pedido`):
* **Interface e Modelo:** Ficam na camada de **Domain**. O modelo de retorno não é um DTO, mas sim um **Value Object**.
* **Implementação:** A **Infrastructure** consome a API externa, converte para o `Value Object` e o devolve para o Domínio.

---

## 3. Entidades vs. Value Objects no Domínio

As classes do núcleo do sistema dividem-se em:

* **Entidades (Entities):** Possuem uma **Identidade Única (ID)**. Seus atributos podem mudar com o tempo, mas a identidade permanece a mesma (Ex: `Usuario`, `Pedido`).
* **Objetos de Valor (Value Objects):** Não possuem ID. São definidos por seus valores. São imutáveis e substituídos por inteiro caso algo mude (Ex: `Endereco`, `CPF`, `Dinheiro`).
  * **No .NET 9:** Utilize `record` para modelar Value Objects, pois eles já trazem imutabilidade e igualdade por valor nativamente.

```csharp
// Exemplo de Value Object usando Record no .NET 9
public record Endereco(string Cep, string Logradouro, string Estado)
{
    // Auto-validação de negócio no construtor
    public Endereco {
        if (string.IsNullOrWhiteSpace(Cep) || Cep.Length != 8)
            throw new ArgumentException("CEP inválido");
    }
}
```

---

## 4. Camada Cross-Cutting (Shared Kernel)
Serviços e utilitários técnicos genéricos (formatação de strings, extensions, reflection, validações matemáticas como Módulo 11 de CPF) pertencem à camada **Cross-Cutting**.

* **A Regra de Ouro (Pureza):** Para que a camada de **Domain** possa referenciar o **Cross-Cutting**, o projeto Cross-Cutting deve conter **C# puro**. 
* **Proibido:** Não instale pacotes de infraestrutura (Entity Framework, Serilog, ASP.NET HttpContext) no Cross-Cutting base. Se precisar centralizar a injeção de dependência de bibliotecas externas, crie um projeto separado (ex: `App.CrossCutting.IoC`) que será referenciado apenas pela Infraestrutura/API, **nunca pelo Domínio**.

---

## 5. Controladores (Controllers) e Application
A Controller na camada de API atua apenas como um "Adaptador" de HTTP (o garçom).

* A **Application** define as Portas de Entrada (Interfaces como `IUsuarioAppService` ou comandos do `MediatR`).
* A **Controller** injeta essas portas, recebe o request, despacha para a Application e retorna o HTTP Status Code adequado.
* A **Controller** nunca implementa lógica de negócio e nunca orquestra repositórios diretamente.

---

## 6. Diagrama de Dependências de Projetos (.csproj)
A regra de dependência sempre aponta para o centro (Domain). Nenhuma camada inferior pode referenciar uma camada superior.

1. **`App.CrossCutting` (Shared)**
   * *O que tem:* C# puro (Extensions, Enums globais, Records base).
   * *Depende de:* Ninguém (Apenas .NET BCL).
2. **`App.Domain`**
   * *O que tem:* Entidades, Value Objects, Interfaces de Repositórios e Serviços de Domínio.
   * *Depende de:* `App.CrossCutting`.
3. **`App.Application`**
   * *O que tem:* Use Cases, AppServices, DTOs, Queries (Interfaces), Interfaces de APIs externas para UI.
   * *Depende de:* `App.Domain` e `App.CrossCutting`.
4. **`App.Infrastructure`**
   * *O que tem:* Entity Framework (DbContext, Mappings), Repositórios concretos, Dapper, Chamadas HTTP (HttpClient para ViaCEP/Correios).
   * *Depende de:* `App.Application`, `App.Domain` e `App.CrossCutting`.
5. **`App.Api`**
   * *O que tem:* Controllers, Program.cs, Middlewares.
   * *Depende de:* `App.Application`, `App.CrossCutting` e `App.Infrastructure` *(a Infra é referenciada aqui apenas para configurar a Injeção de Dependência no Program.cs)*.

---
*Documento gerado como base arquitetural para padronização da API .NET 9.*

--- 

### Como salvar este documento:
1. Copie todo o texto acima (que está dentro dos blocos `---`).
2. Vá no seu Visual Studio / VS Code.
3. Crie um novo arquivo chamado `ARCHITECTURE.md` na raiz do seu repositório.
4. Cole o texto e salve. 

Sempre que bater aquela dúvida se algo deve ir para a Infra, Application ou CrossCutting, você pode consultar esse arquivo rapidamente! Se precisar de mais alguma coisa, conte comigo.
