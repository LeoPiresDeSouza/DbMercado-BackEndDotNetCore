using DbMercado.Domain.Administracao.Entities;
using Microsoft.AspNetCore.Identity;
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

        context.SaveChanges();
    }



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
