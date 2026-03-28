Quando o seu DbContext está em um projeto de biblioteca de classes (ex: MeuProjeto.Data) e o projeto que roda a aplicação é outro (ex: MeuProjeto.API), você deve informar explicitamente os caminhos ao terminal.
Aqui estão os comandos detalhados. Considere que você está na raiz da solução (pasta onde fica o arquivo .sln).
1. Criar a Migration
Este comando diz ao EF: "Olhe para o projeto de inicialização para pegar a configuração, mas salve o código da migration no projeto de dados".
code
Bash
dotnet ef migrations add NomeDaMigration --project NomeDoProjetoData --startup-project NomeDoProjetoAPI
2. Aplicar a Migration no Banco de Dados
Este comando diz ao EF: "Execute as migrações que estão no projeto de dados usando a string de conexão que está no projeto de inicialização".
code
Bash
dotnet ef database update --project NomeDoProjetoData --startup-project NomeDoProjetoAPI
O que significa cada parâmetro:
--project (ou -p): É o projeto onde está o seu DbContext e onde a pasta Migrations será criada. Você deve passar o nome da pasta do projeto ou o caminho para o arquivo .csproj.
--startup-project (ou -s): É o projeto que tem o appsettings.json e o Program.cs. O EF Core precisa "rodar" esse projeto temporariamente para descobrir qual banco de dados usar e qual a string de conexão.
Exemplo Prático
Se a estrutura de pastas for:
Solucao/
Ecommerce.Data/ (Aqui está o Context)
Ecommerce.API/ (Aqui está o appsettings.json)
Os comandos seriam:
code
Bash
# Criar
dotnet ef migrations add InitialCreate --project Ecommerce.Data --startup-project Ecommerce.API

# Aplicar
dotnet ef database update --project Ecommerce.Data --startup-project Ecommerce.API
E se eu usar o "Console do Gerenciador de Pacotes" (Visual Studio)?
Se você prefere usar o console interno do Visual Studio (aquele com comandos em PowerShell), os comandos são:
Criar Migration:
code
Powershell
Add-Migration NomeDaMigration -Project Ecommerce.Data -StartupProject Ecommerce.API
Atualizar Banco:
code
Powershell
Update-Database -Project Ecommerce.Data -StartupProject Ecommerce.API
Dica Extra: Certifique-se de que o pacote Microsoft.EntityFrameworkCore.Design esteja instalado no projeto de Inicialização (API), caso contrário, o comando --startup-project falhará.
