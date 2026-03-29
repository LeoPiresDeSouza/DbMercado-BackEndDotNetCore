using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Claims;

namespace DbMercado.Infrastructure.Shared.Data;

#pragma warning disable

public class DbInitializer
{
    internal static void Initialize(AppDbContext context,
                                    ILoggerFactory logger,
                                    IServiceProvider provider)
    {
        ILogger _logger = logger.CreateLogger("DbInitializer");

        _logger.LogWarning("SeedDatabase - Initialize");

        try
        {
            var dataCarga = DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("pt-BR"));
            var usuarioCarga = "Carga Inicial";

            AddParametros(context, dataCarga, usuarioCarga);
            AddModulo(context, dataCarga, usuarioCarga);
            AddFuncionalidades(context, dataCarga, usuarioCarga);
            AddPermissoes(context, dataCarga, usuarioCarga);
            AddApplicationUser(context, provider);
            AddProdutosDemonstracao(context, usuarioCarga, _logger);

            _logger.LogWarning("SeedDatabase - Finalizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SeedDatabase - Erro inicializando Database");
        }
    }




    #region Parâmetros

    private static void AddParametros(AppDbContext context, DateTime dataCarga, string usuarioCarga)
    {
        AddParametroIfNotExists(context,
                                "Log",
                                "Limpeza",
                                "DiasLimpeza",
                                "30",
                                "Número de dias para manter os logs antes de serem elegíveis para limpeza.",
                                dataCarga,
                                usuarioCarga);
        AddParametroIfNotExists(context,
                                "Log",
                                "Limpeza",
                                "MinimoRegistros",
                                "5000",
                                "Quantidade mínima de logs a ser mantida na tabela após a sua limpeza.",
                                dataCarga,
                                usuarioCarga);

        // Tipo de Sexo

        AddParametroIfNotExists(context, "pessoa", "sexo", "tipo", "Masculino", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "sexo", "tipo", "Feminino", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "sexo", "tipo", "Não binário", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "sexo", "tipo", "Outros", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "sexo", "tipo", "Prefiro não informar", null, dataCarga, usuarioCarga);

        // Tipo de telefone

        AddParametroIfNotExists(context, "pessoa", "telefone", "tipo", "Celular", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "telefone", "tipo", "Fixo", null, dataCarga, usuarioCarga);

        // Tipo de Documento de pessoa Física

        AddParametroIfNotExists(context, "pessoa", "documento", "tipo", "CPF", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "documento", "tipo", "Passaporte", null, dataCarga, usuarioCarga);

        // Redes Sociais

        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "Instagram", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "Facebook", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "LinkedIn", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "X", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "Youtube", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "TikTok", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "GitHub", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "pessoa", "redeSocial", "nome", "Site", null, dataCarga, usuarioCarga);

        AddParametrosProduto(context, dataCarga, usuarioCarga);

        context.SaveChanges();
    }

    /// <summary>Unidades de medida/peso e demais domínios de produto (chave = código persistido no produto).</summary>
    private static void AddParametrosProduto(AppDbContext context, DateTime dataCarga, string usuarioCarga)
    {
        foreach (var (codigo, rotulo) in UnidadesMedidaProdutoSeed)
            AddParametroIfNotExists(context, "produto", "unidadeMedida", codigo, rotulo, null, dataCarga, usuarioCarga);

        AddParametroIfNotExists(context, "produto", "origemGeografica", "NACIONAL", "Nacional", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemGeografica", "IMPORTADO", "Importado", null, dataCarga, usuarioCarga);

        AddParametroIfNotExists(context, "produto", "origemIcms", "0", "Nacional, exceto as indicadas nos códigos 3 a 5", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "1", "Estrangeira — importação direta, exceto a indicada no código 6", null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "2", "Estrangeira — adquirida no mercado interno, exceto a indicada no código 7", null, dataCarga, usuarioCarga);
    }

    /// <summary>Códigos comercialmente usuais (ex.: documentos fiscais e logística); novas unidades = novo registro em parâmetro.</summary>
    private static readonly (string Codigo, string Rotulo)[] UnidadesMedidaProdutoSeed =
    {
        ("UN", "UN"), ("NIU", "NIU"), ("PC", "PC"), ("PCT", "PCT"), ("PAR", "PAR"), ("PR", "PR"),
        ("DZ", "DZ"), ("DUZIA", "DUZIA"), ("CJ", "CJ"), ("KIT", "KIT"), ("SET", "SET"), ("ROL", "ROL"),
        ("BG", "BG"), ("TB", "TB"), ("BX", "BX"), ("CX", "CX"), ("FD", "FD"), ("SC", "SC"), ("SAC", "SAC"),
        ("LAT", "LAT"), ("PT", "PT"), ("POT", "POT"), ("GL", "GL"), ("BR", "BR"), ("BAL", "BAL"), ("BL", "BL"),
        ("FR", "FR"),
        ("KG", "KG"), ("KGM", "KGM"), ("G", "G"), ("GRM", "GRM"), ("MG", "MG"), ("TON", "TON"), ("TNE", "TNE"),
        ("T", "T"),
        ("L", "L"), ("LT", "LT"), ("LTR", "LTR"), ("ML", "ML"), ("MLT", "MLT"),
        ("M3", "M3"), ("MTQ", "MTQ"), ("M2", "M2"), ("MTK", "MTK"), ("M", "M"), ("MTR", "MTR"), ("CMT", "CMT"),
        ("CM", "CM"), ("MMT", "MMT"), ("MM", "MM"), ("KMT", "KMT"), ("KM", "KM"),
        ("H", "H"), ("HR", "HR"), ("DIA", "DIA"), ("MIN", "MIN"), ("S", "S"), ("ANO", "ANO")
    };



    private static void AddParametroIfNotExists(AppDbContext context,
                                                string categoria,
                                                string atributo,
                                                string chave,
                                                string valor,
                                                string? descricao,
                                                DateTime dataCarga,
                                                string usuarioCarga)
    {
        if (context.Parametros.Any(m => m.Categoria == categoria &&
                                        m.Atributo == atributo &&
                                        m.Chave == chave &&
                                        m.Valor == valor))
            return;

        context.Parametros.Add(new ParametroEntity
        {
            Categoria = categoria,
            Atributo = atributo,
            Chave = chave,
            Valor = valor,
            Descricao = descricao,
            DataCriacao = dataCarga,
            DataUltimaAlteracao = dataCarga,
            UsuarioCriacao = usuarioCarga,
            UsuarioUltimaAlteracao = usuarioCarga
        });
    }

    #endregion Parâmetros




    #region Produtos demonstração (seed incremental)

    /// <summary>Prefixo de nome nos itens de demonstração (apenas identificação humana; idempotência é por GTIN fixo por item).</summary>
    private const string ProdutoSeedMarcadorNome = "[seed] ";

    /// <summary>
    /// Carga incremental de produtos fictícios (um registro por GTIN, se ainda não existir).
    /// Se as tabelas de produto não existirem (erro 208), registra aviso: aplicar migrações EF (<c>prdProduto</c>).
    /// </summary>
    private static void AddProdutosDemonstracao(AppDbContext context, string usuarioCarga, ILogger logger)
    {
        try
        {
            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7893500030518", () =>
                ProdutoEntity.Registrar(
                    nome: $"{ProdutoSeedMarcadorNome}Arroz parboilizado Tio João 1 kg",
                    descricao: "Arroz longo fino tipo 1, embalagem plástica. Dados ilustrativos para ambiente de demonstração.",
                    marca: "Tio João",
                    modelo: "Tipo 1",
                    gtin: "7893500030518",
                    unidadeMedida: "UN",
                    dimensaoProduto: DimensaoProduto.Criar(0.04m, 0.15m, 0.22m),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.045m, 0.16m, 0.23m, 1.05m),
                    origemProduto: OrigemProduto.Criar("NACIONAL", null),
                    dadosFiscais: DadosFiscais.Criar("10063021", "1705500", "0"),
                    atributosIniciais: new[]
                    {
                        AtributoProduto.Criar("Armazenamento", "Local seco e arejado"),
                        AtributoProduto.Criar("Validade típica", "12 meses (referência fictícia)")
                    },
                    skusIniciais: new[] { ("ARZ-TJ-1KG-UN", true), ("ARZ-TJ-1KG-CX12", true) },
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7896048320065", () =>
                ProdutoEntity.Registrar(
                    nome: $"{ProdutoSeedMarcadorNome}Azeite extra virgem Andorinha 500 ml",
                    descricao: "Azeite de oliva extra virgem, vidro. Demonstração — não é oferta comercial.",
                    marca: "Andorinha",
                    modelo: "Extra virgem",
                    gtin: "7896048320065",
                    unidadeMedida: "UN",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.22m, 0.07m, 0.07m, 0.85m),
                    origemProduto: OrigemProduto.Criar("NACIONAL", null),
                    dadosFiscais: DadosFiscais.Criar("15091000", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Volume", "500 ml") },
                    skusIniciais: new[] { ("AZE-AND-500ML", true) },
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7891234567890", () =>
                ProdutoEntity.Registrar(
                    nome: $"{ProdutoSeedMarcadorNome}Notebook 14\" fictício — importado",
                    descricao: "Equipamento de informática para testes de origem IMPORTADO e origem ICMS 1.",
                    marca: "TechDemo",
                    modelo: "Book14-Mock",
                    gtin: "7891234567890",
                    unidadeMedida: "UN",
                    dimensaoProduto: DimensaoProduto.Criar(0.02m, 0.32m, 0.22m),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.08m, 0.38m, 0.28m, 2.2m),
                    origemProduto: OrigemProduto.Criar("IMPORTADO", "China"),
                    dadosFiscais: DadosFiscais.Criar("84713012", "2108700", "1"),
                    atributosIniciais: new[] { AtributoProduto.Criar("CPU", "Mock i5"), AtributoProduto.Criar("RAM", "8 GB") },
                    skusIniciais: new[] { ("NB-DEMO-14-I5", true), ("NB-DEMO-14-I5-REF", false) },
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7891000100103", () =>
                ProdutoEntity.Registrar(
                    nome: $"{ProdutoSeedMarcadorNome}Detergente líquido limpeza total 500 ml",
                    descricao: "Agente de limpeza — caixa com múltiplas unidades (SKU principal por frasco).",
                    marca: "LimpaBem",
                    modelo: "Neutro",
                    gtin: "7891000100103",
                    unidadeMedida: "CX",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.25m, 0.32m, 0.40m, 6.5m),
                    origemProduto: OrigemProduto.Criar("NACIONAL", null),
                    dadosFiscais: DadosFiscais.Criar("34022000", "2803800", "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Fragrância", "Limão"), AtributoProduto.Criar("pH", "~7") },
                    skusIniciais: new[] { ("DET-LIM-500-CX24", true) },
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7896004001234", () =>
                ProdutoEntity.Registrar(
                    nome: $"{ProdutoSeedMarcadorNome}Café torrado em grãos especial 250 g",
                    descricao: "Café arábica torrado, acondicionado a vácuo. Peso líquido 250 g (unidade de venda: pacote).",
                    marca: "Café do Cerrado",
                    modelo: "Grãos inteiros",
                    gtin: "7896004001234",
                    unidadeMedida: "UN",
                    dimensaoProduto: DimensaoProduto.Criar(0.03m, 0.12m, 0.18m),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.035m, 0.13m, 0.19m, 0.26m),
                    origemProduto: OrigemProduto.Criar("NACIONAL", null),
                    dadosFiscais: DadosFiscais.Criar("09011100", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Torra", "Média"), AtributoProduto.Criar("Safra", "Referência demo") },
                    skusIniciais: new[] { ("CAF-CER-250G", true), ("CAF-CER-250G-ORG", true) },
                    usuarioAuditoria: usuarioCarga));

            if (context.ChangeTracker.HasChanges())
                context.SaveChanges();
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            logger.LogWarning(ex,
                "Seed incremental de produtos demonstração ignorado: tabela ou objeto não encontrado (erro SQL 208). " +
                "Aplique as migrações do Entity Framework para criar prdProduto / prdProdutoAtributo (ex.: dotnet ef database update no projeto da API).");
        }
        catch (Exception ex) when (ContemSqlErroObjetoInvalido(ex))
        {
            logger.LogWarning(ex,
                "Seed incremental de produtos demonstração ignorado: falha de acesso ao esquema de produto. Verifique migrações EF e existência de prdProduto.");
        }
    }

    private static bool ContemSqlErroObjetoInvalido(Exception ex)
    {
        for (Exception? e = ex; e != null; e = e.InnerException)
        {
            if (e is SqlException sql && sql.Number == 208)
                return true;
        }

        return false;
    }

    /// <summary>Inclui um produto de demonstração somente se não existir registro com o mesmo GTIN (carga incremental idempotente).</summary>
    private static void AddProdutoDemonstracaoIfNotExistsPorGtin(
        AppDbContext context,
        string usuarioCarga,
        string gtin,
        Func<ProdutoEntity> fabrica)
    {
        if (context.Produtos.Any(p => p.Gtin == gtin))
            return;

        context.Produtos.Add(fabrica());
    }

    #endregion Produtos demonstração (seed incremental)





    #region Identity

    private static void AddApplicationUser(AppDbContext context, IServiceProvider provider)
    {
        var usermanager = provider.GetRequiredService<UserManager<IdentityUser>>();

        string email = "lps064@gmail.com";
        string senha = "Leo@123";

        var user = usermanager.FindByEmailAsync(email).Result;

        if (user == null)
        {
            var newUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            usermanager.CreateAsync(newUser, senha).Wait();

            user = usermanager.FindByEmailAsync(email).Result;

            var code = usermanager.GenerateEmailConfirmationTokenAsync(user).Result;

            usermanager.ConfirmEmailAsync(user, code).Wait();
        }

        // Garantir que o usuário tenha todas as permissões existentes

        AddAllPermissionsToUser(context, usermanager, user);

        AssociarPermissoesCadastroProdutosAoUsuario(context, usermanager, email);
    }



    private static void AddAllPermissionsToUser(AppDbContext context,
                                                UserManager<IdentityUser> usermanager,
                                                IdentityUser user)
    {
        string claimPermissaoKey = "Permissao";

        var permissoes = context.Permissoes
            .Select(p => p.Id)
            .ToList();

        var userClaims = usermanager.GetClaimsAsync(user).Result
            .Where(c => c.Type == claimPermissaoKey)
            .Select(c => c.Value)
            .ToList();

        var novasPermissoes = permissoes
            .Where(p => !userClaims.Contains(p.ToString()))
            .ToList();

        if (!novasPermissoes.Any())
            return;

        var claims = novasPermissoes
            .Select(p => new Claim(claimPermissaoKey, p.ToString()))
            .ToList();

        usermanager.AddClaimsAsync(user, claims).Wait();
    }

    private static void AssociarPermissoesCadastroProdutosAoUsuario(AppDbContext context,
                                                                   UserManager<IdentityUser> usermanager,
                                                                   string email)
    {
        var usuario = usermanager.FindByEmailAsync(email).Result;
        if (usuario is null)
            return;

        const string claimPermissaoKey = "Permissao";

        var funcionalidade = context.Funcionalidades
            .FirstOrDefault(f => f.NomeNormalizado == "cadastroprodutos");
        if (funcionalidade is null)
            return;

        var idsPermissao = context.Permissoes
            .Where(p => p.FuncionalidadeId == funcionalidade.Id)
            .Select(p => p.Id)
            .ToList();

        var claimsExistentes = usermanager.GetClaimsAsync(usuario).Result
            .Where(c => c.Type == claimPermissaoKey)
            .Select(c => c.Value)
            .ToList();

        var novas = idsPermissao
            .Where(id => !claimsExistentes.Contains(id.ToString()))
            .ToList();

        if (!novas.Any())
            return;

        var claims = novas
            .Select(id => new Claim(claimPermissaoKey, id.ToString()))
            .ToList();

        usermanager.AddClaimsAsync(usuario, claims).Wait();
    }

    #endregion




    #region Modulos

    private static void AddModulo(AppDbContext context, DateTime dataCarga, string usuarioCarga)
    {
        AddModuloIfNotExists(context,
            "administracao",
            "Administração",
            "Módulo de Administração do sistema.",
            1,
            dataCarga,
            usuarioCarga);

        AddModuloIfNotExists(context,
            "produtos",
            "Produtos",
            "Módulo de gestão do cadastro, controle de estoque, depósitos e movimentações de produtos.",
            2,
            dataCarga,
            usuarioCarga);

        context.SaveChanges();
    }

    private static void AddModuloIfNotExists(AppDbContext context,
                                             string nomeNormalizado,
                                             string nomeExibicao,
                                             string descricao,
                                             int ordem,
                                             DateTime dataCarga,
                                             string usuarioCarga)
    {
        if (context.Modulos.Any(m => m.NomeNormalizado == nomeNormalizado))
            return;

        context.Modulos.Add(new ModuloEntity
        {
            NomeNormalizado = nomeNormalizado,
            NomeExibicao = nomeExibicao,
            Descricao = descricao,
            OrdemExibicao = ordem,
            Icone = "",
            DataCriacao = dataCarga,
            DataUltimaAlteracao = dataCarga,
            UsuarioCriacao = usuarioCarga,
            UsuarioUltimaAlteracao = usuarioCarga
        });
    }

    #endregion




    #region Funcionalidades

    private static void AddFuncionalidades(AppDbContext context, DateTime dataCarga, string usuarioCarga)
    {
        AddFuncionalidadeIfNotExists(context,
            "administracao",
            "cadastroparametros",
            "Cadastro de Parâmetros",
            "Cadastro dos parâmetros de configuração da aplicação.",
            1,
            dataCarga,
            usuarioCarga);

        AddFuncionalidadeIfNotExists(context,
            "administracao",
            "controledelogs",
            "Controle de Logs",
            "Gestão dos logs da aplicação.",
            2,
            dataCarga,
            usuarioCarga);

        AddFuncionalidadeIfNotExists(context,
            "produtos",
            "cadastroprodutos",
            "Cadastro de Produtos",
            "Cadastro dos produtos.",
            1,
            dataCarga,
            usuarioCarga);

        AddFuncionalidadeIfNotExists(context,
            "produtos",
            "cadastrodecentrosdedistribuicao",
            "Cadastro de Centros de Distribuição",
            "Cadastro dos centros de distribuição.",
            2,
            dataCarga,
            usuarioCarga);

        context.SaveChanges();
    }



    private static void AddFuncionalidadeIfNotExists(AppDbContext context,
                                                     string moduloNome,
                                                     string nomeNormalizado,
                                                     string nomeExibicao,
                                                     string descricao,
                                                     int ordem,
                                                     DateTime dataCarga,
                                                     string usuarioCarga)
    {
        if (context.Funcionalidades.Any(f => f.NomeNormalizado == nomeNormalizado))
            return;

        var modulo = context.Modulos.First(m => m.NomeNormalizado == moduloNome);

        context.Funcionalidades.Add(new FuncionalidadeEntity
        {
            NomeNormalizado = nomeNormalizado,
            NomeExibicao = nomeExibicao,
            Descricao = descricao,
            OrdemExibicao = ordem,
            Icone = "",
            ModuloId = modulo.Id,
            DataCriacao = dataCarga,
            DataUltimaAlteracao = dataCarga,
            UsuarioCriacao = usuarioCarga,
            UsuarioUltimaAlteracao = usuarioCarga
        });
    }

    #endregion




    #region Permissoes

    private static void AddPermissoes(AppDbContext context, DateTime dataCarga, string usuarioCarga)
    {
        AddPermissaoIfNotExists(context, "cadastroparametros", "acessar", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "controledelogs", "acessar", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "controledelogs", "excluirEntrada", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "controledelogs", "limparLog", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "controledelogs", "descarregarParaDisco", dataCarga, usuarioCarga);

        AddPermissaoIfNotExists(context, "cadastroprodutos", "criar", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "cadastroprodutos", "ler", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "cadastroprodutos", "atualizar", dataCarga, usuarioCarga);
        AddPermissaoIfNotExists(context, "cadastroprodutos", "excluir", dataCarga, usuarioCarga);

        context.SaveChanges();
    }



    private static void AddPermissaoIfNotExists(AppDbContext context,
                                                string funcionalidadeNome,
                                                string permissao,
                                                DateTime dataCarga,
                                                string usuarioCarga)
    {
        var funcionalidade = context.Funcionalidades
            .First(f => f.NomeNormalizado == funcionalidadeNome);

        if (context.Permissoes.Any(p =>
            p.FuncionalidadeId == funcionalidade.Id &&
            p.Permissao == permissao))
            return;

        context.Permissoes.Add(new PermissaoEntity
        {
            Permissao = permissao,
            FuncionalidadeId = funcionalidade.Id,
            DataCriacao = dataCarga,
            DataUltimaAlteracao = dataCarga,
            UsuarioCriacao = usuarioCarga,
            UsuarioUltimaAlteracao = usuarioCarga
        });
    }

    #endregion
}
