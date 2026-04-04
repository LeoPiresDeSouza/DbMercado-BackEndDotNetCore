using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
            var mapaCategoria = AddCategorias(context, dataCarga, usuarioCarga);
            AddProdutosDemonstracao(context, usuarioCarga, _logger, mapaCategoria);

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
                                "1",
                                "Número de dias para manter os logs em disco antes de serem excluídos.",
                                dataCarga,
                                usuarioCarga);
        AddParametroIfNotExists(context,
                                "Log",
                                "Limpeza",
                                "MinimoRegistros",
                                "50",
                                "Quantidade mínima de logs a ser mantida na tabela após a sua limpeza.",
                                dataCarga,
                                usuarioCarga);
        AddParametroIfNotExists(context,
                                "Log",
                                "Limpeza",
                                "MaximoRegistros",
                                "100",
                                "Quantidade máxima de logs a ser mantida na tabela após a sua limpeza.",
                                dataCarga,
                                usuarioCarga);
        AddParametroIfNotExists(context,
                                "Log",
                                "Limpeza",
                                "Job",
                                "0 0 3 * * ?",
                                "Expressão cron Quartz (UTC) do job de limpeza de dbAppLog. Lida na inicialização da API; appsettings Quartz:LogCleanup:CronSchedule é fallback se vazio.",
                                dataCarga,
                                usuarioCarga);

        AddParametroIfNotExists(context,
                                "Quartz",
                                "LimpezaMidiasTemporarias",
                                "CronSchedule",
                                "0 30 3 * * ?",
                                "Expressão cron Quartz (UTC) do job LimpezaMidiasTemporarias. Lida na inicialização da API; appsettings é fallback se vazio.",
                                dataCarga,
                                usuarioCarga);

        AddParametroIfNotExists(context,
                                "Quartz",
                                "MarketplaceSync",
                                "CronSchedule",
                                "0 0 * * * ?",
                                "Reservado: cron (UTC) para futuro job de sincronização com marketplaces. Lida na inicialização quando o job existir.",
                                dataCarga,
                                usuarioCarga);

        AddParametroIfNotExists(context,
                                "Quartz",
                                "ConciliacaoFinanceira",
                                "CronSchedule",
                                "0 0 6 * * ?",
                                "Reservado: cron (UTC) para futuro job de conciliação financeira.",
                                dataCarga,
                                usuarioCarga);

        AddParametroIfNotExists(context,
                                "Quartz",
                                "Reprocessamento",
                                "CronSchedule",
                                "0 0/30 * * * ?",
                                "Reservado: cron (UTC) para futuro job de reprocessamento.",
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

    private static void AddParametrosProduto(AppDbContext context, DateTime dataCarga, string usuarioCarga)
    {
        // ─── Unidade de comercialização — como o produto é vendido/faturado ───
        var unidadesCom = new[]
        {
            ("UN",  "Unidade"),
            ("KIT", "Kit"),
            ("DZ",  "Dúzia"),
            ("PAR", "Par"),
            ("CX",  "Caixa"),
            ("PCT", "Pacote"),
            ("FD",  "Fardo"),
            ("SC",  "Saco"),
            ("ROL", "Rolo"),
            ("M",   "Metro"),
            ("KG",  "Quilograma"),
            ("L",   "Litro"),
        };
        foreach (var (c, v) in unidadesCom)
            AddParametroIfNotExists(context, "produto", "unidadeComercializacao", c, v, null, dataCarga, usuarioCarga);

        // ─── Unidade de medida física — natureza do produto (NF-e / fiscal) ───
        var unidadesMedida = new[]
        {
            ("UN",  "Unidade"),
            ("KG",  "Quilograma"),
            ("G",   "Grama"),
            ("T",   "Tonelada"),
            ("L",   "Litro"),
            ("ML",  "Mililitro"),
            ("M",   "Metro"),
            ("CM",  "Centímetro"),
            ("MM",  "Milímetro"),
            ("M2",  "Metro quadrado"),
            ("M3",  "Metro cúbico"),
        };
        foreach (var (c, v) in unidadesMedida)
            AddParametroIfNotExists(context, "produto", "unidadeMedida", c, v, null, dataCarga, usuarioCarga);

        // ─── Unidade de embalagem — tipo de acondicionamento ───
        var unidadesEmb = new[]
        {
            ("CX",  "Caixa"),
            ("FD",  "Fardo"),
            ("PCT", "Pacote"),
            ("SC",  "Saco"),
            ("SAC", "Sacola"),
            ("LAT", "Lata"),
            ("GL",  "Galão"),
            ("FR",  "Frasco"),
            ("PT",  "Pote"),
            ("ROL", "Rolo"),
            ("TB",  "Tubo"),
            ("BL",  "Blister"),
        };
        foreach (var (c, v) in unidadesEmb)
            AddParametroIfNotExists(context, "produto", "unidadeEmbalagem", c, v, null, dataCarga, usuarioCarga);

        // ─── Unidade de dimensão — para altura, largura, comprimento ───
        var unidadesDim = new[]
        {
            ("CM", "Centímetro (cm)"),
            ("M",  "Metro (m)"),
            ("MM", "Milímetro (mm)"),
        };
        foreach (var (c, v) in unidadesDim)
            AddParametroIfNotExists(context, "produto", "unidadeDimensao", c, v, null, dataCarga, usuarioCarga);

        // ─── Unidade de peso — para peso bruto logístico ───
        var unidadesPeso = new[]
        {
            ("KG", "Quilograma (kg)"),
            ("G",  "Grama (g)"),
            ("T",  "Tonelada (t)"),
        };
        foreach (var (c, v) in unidadesPeso)
            AddParametroIfNotExists(context, "produto", "unidadePeso", c, v, null, dataCarga, usuarioCarga);

        // ─── Origem geográfica (chaves = OrigemGeograficaProdutoCodigos) ───
        AddParametroIfNotExists(context, "produto", "origemGeografica", "1", "Nacional",  null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemGeografica", "2", "Importado", null, dataCarga, usuarioCarga);

        // ─── Origem ICMS (SEFAZ): atributo produto/origemIcms, códigos 0–8 — exatamente uma linha por Chave ───
        // AddParametroIfNotExists não serve aqui: ele só bloqueia (categoria+atributo+chave+valor) repetido; a mesma Chave com outro texto virava segunda linha.
        var origensIcmsSefaz = new (string chave, string valor)[]
        {
            ("0", "0 — Nacional, exceto códigos 3 a 5"),
            ("1", "1 — Estrangeira, importação direta, exceto código 6"),
            ("2", "2 — Estrangeira, adquirida no mercado interno, exceto código 7"),
            ("3", "3 — Nacional, conteúdo de importação > 40% e ≤ 70%"),
            ("4", "4 — Nacional, processo produtivo básico (PPB)"),
            ("5", "5 — Nacional, conteúdo de importação ≤ 40%"),
            ("6", "6 — Estrangeira, importação direta sem similar nacional"),
            ("7", "7 — Estrangeira, mercado interno sem similar nacional"),
            ("8", "8 — Nacional, conteúdo de importação > 70%"),
        };
        foreach (var (chave, valor) in origensIcmsSefaz)
            AddParametroProdutoOrigemIcmsSeChaveLivre(context, chave, valor, null, dataCarga, usuarioCarga);
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

    /// <summary>Insere produto/origemIcms só se ainda não existir linha com a mesma <paramref name="chave"/> (seed SEFAZ 0–8).</summary>
    private static void AddParametroProdutoOrigemIcmsSeChaveLivre(
        AppDbContext context,
        string chave,
        string valor,
        string? descricao,
        DateTime dataCarga,
        string usuarioCarga)
    {
        const string categoria = "produto";
        const string atributo = "origemIcms";
        if (context.Parametros.Any(m =>
                m.Categoria == categoria && m.Atributo == atributo && m.Chave == chave))
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

    /// <summary>Prefixo antigo gravado no <c>Nome</c> em ambientes já semeados antes da remoção do marcador no código.</summary>
    private const string LegadoSeedPrefixoNome = "[seed] ";

    /// <summary>
    /// Remove o prefixo legado <c>[seed] </c> dos nomes já persistidos (o seed novo não adiciona mais esse texto).
    /// </summary>
    private static void CorrigirNomesProdutosComPrefixoLegadoSeed(AppDbContext context, ILogger logger)
    {
        try
        {
            var len = LegadoSeedPrefixoNome.Length;
            var affected = context.Database.ExecuteSqlRaw(
                "UPDATE prdProduto SET Nome = STUFF(Nome, 1, @prefixLen, N'') WHERE LEFT(Nome, @prefixLen) = @prefix",
                new SqlParameter("@prefixLen", len),
                new SqlParameter("@prefix", LegadoSeedPrefixoNome));

            if (affected > 0)
            {
                logger.LogInformation(
                    "Nomes de produto: removido prefixo legado '{Prefix}' de {Count} registro(s) em prdProduto.",
                    LegadoSeedPrefixoNome.TrimEnd(),
                    affected);
            }
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            // tabela ainda não existe
        }
        catch (Exception ex) when (ContemSqlErroObjetoInvalido(ex))
        {
            logger.LogDebug(ex, "Correção de prefixo em prdProduto ignorada (objeto inválido).");
        }
    }

    /// <summary>
    /// Seed incremental de categorias alinhado a <c>docs/REVISAO_SEEDER_CATEGFORIAS.md</c> (até 4 níveis).
    /// Nomes de ramos repetidos no documento foram diferenciados para garantir slugs únicos (<see cref="EnsureCategoria"/>).
    /// </summary>
    private static Dictionary<string, long> AddCategorias(
        AppDbContext context,
        DateTime dataCarga,
        string usuarioCarga)
    {
        var mapa = new Dictionary<string, long>();

        // ── Nível 1 ──
        var fitness = EnsureCategoria(context, "Fitness", null, dataCarga, usuarioCarga);
        var eletronicos = EnsureCategoria(context, "Eletrônicos", null, dataCarga, usuarioCarga);
        var utilidadesDomesticas = EnsureCategoria(context, "Utilidades Domésticas", null, dataCarga, usuarioCarga);
        var celularesTelefonia = EnsureCategoria(context, "Celulares e Telefonia", null, dataCarga, usuarioCarga);
        var informatica = EnsureCategoria(context, "Informática", null, dataCarga, usuarioCarga);
        context.SaveChanges();

        // ── Nível 2 ──
        var musculacao = EnsureCategoria(context, "Musculação", fitness.Id, dataCarga, usuarioCarga);
        var cardio = EnsureCategoria(context, "Cardio", fitness.Id, dataCarga, usuarioCarga);
        var funcional = EnsureCategoria(context, "Funcional", fitness.Id, dataCarga, usuarioCarga);
        var yogaPilates = EnsureCategoria(context, "Yoga & Pilates", fitness.Id, dataCarga, usuarioCarga);

        var audio = EnsureCategoria(context, "Áudio", eletronicos.Id, dataCarga, usuarioCarga);
        var video = EnsureCategoria(context, "Vídeo", eletronicos.Id, dataCarga, usuarioCarga);
        var seguranca = EnsureCategoria(context, "Segurança", eletronicos.Id, dataCarga, usuarioCarga);
        var acessoriosEletronicos = EnsureCategoria(context, "Acessórios Eletrônicos", eletronicos.Id, dataCarga, usuarioCarga);

        var cozinha = EnsureCategoria(context, "Cozinha", utilidadesDomesticas.Id, dataCarga, usuarioCarga);
        var organizacao = EnsureCategoria(context, "Organização", utilidadesDomesticas.Id, dataCarga, usuarioCarga);
        var limpeza = EnsureCategoria(context, "Limpeza", utilidadesDomesticas.Id, dataCarga, usuarioCarga);
        var lavanderia = EnsureCategoria(context, "Lavanderia", utilidadesDomesticas.Id, dataCarga, usuarioCarga);

        var smartphones = EnsureCategoria(context, "Smartphones", celularesTelefonia.Id, dataCarga, usuarioCarga);
        var acessoriosTelefonia = EnsureCategoria(context, "Acessórios para Telefonia", celularesTelefonia.Id, dataCarga, usuarioCarga);
        var pecasReposicao = EnsureCategoria(context, "Peças e Reposição", celularesTelefonia.Id, dataCarga, usuarioCarga);

        var computadores = EnsureCategoria(context, "Computadores", informatica.Id, dataCarga, usuarioCarga);
        var perifericos = EnsureCategoria(context, "Periféricos", informatica.Id, dataCarga, usuarioCarga);
        var componentes = EnsureCategoria(context, "Componentes", informatica.Id, dataCarga, usuarioCarga);
        var redes = EnsureCategoria(context, "Redes", informatica.Id, dataCarga, usuarioCarga);
        context.SaveChanges();

        // ── Nível 3 ──
        var pesosLivres = EnsureCategoria(context, "Pesos Livres", musculacao.Id, dataCarga, usuarioCarga);
        var maquinas = EnsureCategoria(context, "Máquinas", musculacao.Id, dataCarga, usuarioCarga);
        var equipamentosCardio = EnsureCategoria(context, "Equipamentos para Cardio", cardio.Id, dataCarga, usuarioCarga);
        var acessoriosTreinoFuncional = EnsureCategoria(context, "Acessórios para Treino Funcional", funcional.Id, dataCarga, usuarioCarga);
        var equipamentosYoga = EnsureCategoria(context, "Equipamentos para Yoga e Pilates", yogaPilates.Id, dataCarga, usuarioCarga);

        var equipamentosAudio = EnsureCategoria(context, "Equipamentos de Áudio", audio.Id, dataCarga, usuarioCarga);
        var fones = EnsureCategoria(context, "Fones", audio.Id, dataCarga, usuarioCarga);
        var televisores = EnsureCategoria(context, "Televisores", video.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Projetores", video.Id, dataCarga, usuarioCarga);
        var monitoramento = EnsureCategoria(context, "Monitoramento", seguranca.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Cabos", acessoriosEletronicos.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Adaptadores", acessoriosEletronicos.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Fontes", acessoriosEletronicos.Id, dataCarga, usuarioCarga);

        var utensilios = EnsureCategoria(context, "Utensílios", cozinha.Id, dataCarga, usuarioCarga);
        var panelas = EnsureCategoria(context, "Panelas", cozinha.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Caixas Organizadoras", organizacao.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Organizadores de Armário", organizacao.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Organizadores Multiuso", organizacao.Id, dataCarga, usuarioCarga);
        var equipamentosLimpeza = EnsureCategoria(context, "Equipamentos para Limpeza", limpeza.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Produtos de Limpeza", limpeza.Id, dataCarga, usuarioCarga);
        var acessoriosLavanderia = EnsureCategoria(context, "Acessórios de Lavanderia", lavanderia.Id, dataCarga, usuarioCarga);

        _ = EnsureCategoria(context, "Android", smartphones.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "iOS", smartphones.Id, dataCarga, usuarioCarga);
        var protecaoTelefonia = EnsureCategoria(context, "Proteção", acessoriosTelefonia.Id, dataCarga, usuarioCarga);
        var energiaTelefonia = EnsureCategoria(context, "Energia", acessoriosTelefonia.Id, dataCarga, usuarioCarga);
        var audioMovel = EnsureCategoria(context, "Áudio Móvel", acessoriosTelefonia.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Baterias", pecasReposicao.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Telas", pecasReposicao.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Conectores", pecasReposicao.Id, dataCarga, usuarioCarga);

        _ = EnsureCategoria(context, "Notebooks", computadores.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Desktops", computadores.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Teclados", perifericos.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Mouses", perifericos.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Monitores", perifericos.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Memória RAM", componentes.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "SSD e HD", componentes.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Placas de Vídeo", componentes.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Roteadores", redes.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Switches", redes.Id, dataCarga, usuarioCarga);
        context.SaveChanges();

        // ── Nível 4 (folhas) ──
        _ = EnsureCategoria(context, "Halteres", pesosLivres.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Barras", pesosLivres.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Anilhas", pesosLivres.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Estações de Musculação", maquinas.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Leg Press", maquinas.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Supino", maquinas.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Esteiras", equipamentosCardio.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Bicicletas Ergométricas", equipamentosCardio.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Elípticos", equipamentosCardio.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Kettlebell", acessoriosTreinoFuncional.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Cordas", acessoriosTreinoFuncional.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Medicine Ball", acessoriosTreinoFuncional.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Colchonetes", equipamentosYoga.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Bolas", equipamentosYoga.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Faixas Elásticas", equipamentosYoga.Id, dataCarga, usuarioCarga);

        _ = EnsureCategoria(context, "Caixas de Som", equipamentosAudio.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Soundbars", equipamentosAudio.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Amplificadores", equipamentosAudio.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "In-Ear", fones.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Over-Ear", fones.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Bluetooth", fones.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "LED", televisores.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "OLED", televisores.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "QLED", televisores.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Câmeras IP", monitoramento.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "DVR e NVR", monitoramento.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Kits Segurança", monitoramento.Id, dataCarga, usuarioCarga);

        _ = EnsureCategoria(context, "Talheres", utensilios.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Espátulas", utensilios.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Conchas", utensilios.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Alumínio", panelas.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Inox", panelas.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Antiaderente", panelas.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Vassouras", equipamentosLimpeza.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Mops", equipamentosLimpeza.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Baldes", equipamentosLimpeza.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Varais", acessoriosLavanderia.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Pregadores", acessoriosLavanderia.Id, dataCarga, usuarioCarga);

        _ = EnsureCategoria(context, "Capas", protecaoTelefonia.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Películas", protecaoTelefonia.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Carregadores", energiaTelefonia.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Power Banks", energiaTelefonia.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Fones Bluetooth", audioMovel.Id, dataCarga, usuarioCarga);
        context.SaveChanges();

        foreach (var cat in context.CategoriasProduto.ToList())
            mapa[cat.Slug] = cat.Id;

        return mapa;
    }

    private static CategoriaProdutoEntity EnsureCategoria(
        AppDbContext context,
        string nome,
        long? paiId,
        DateTime dataCarga,
        string usuarioCarga)
    {
        var slug = CategoriaProdutoEntity.GerarSlug(nome);
        var existente = context.CategoriasProduto.FirstOrDefault(c => c.Slug == slug);
        if (existente is not null)
            return existente;

        CategoriaProdutoEntity entidade;
        if (paiId.HasValue)
        {
            var pai = context.CategoriasProduto.First(c => c.Id == paiId.Value);
            entidade = CategoriaProdutoEntity.CriarFilha(pai, nome, null, usuarioCarga);
        }
        else
        {
            entidade = CategoriaProdutoEntity.CriarRaiz(nome, null, usuarioCarga);
        }

        entidade.DataCriacao = dataCarga;
        entidade.DataUltimaAlteracao = dataCarga;

        context.CategoriasProduto.Add(entidade);
        return entidade;
    }

    private static long? IdCategoriaPorSlug(Dictionary<string, long> mapa, string slug) =>
        mapa.TryGetValue(slug, out var id) ? id : null;

    /// <summary>
    /// Carga incremental de produtos fictícios (um registro por GTIN, se ainda não existir).
    /// Se as tabelas de produto não existirem (erro 208), registra aviso: aplicar migrações EF (<c>prdProduto</c>).
    /// </summary>
    private static void AddProdutosDemonstracao(AppDbContext context, string usuarioCarga, ILogger logger, Dictionary<string, long> mapaCategoria)
    {
        try
        {
            CorrigirNomesProdutosComPrefixoLegadoSeed(context, logger);

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7891234567890", () =>
                ProdutoEntity.Registrar(
                    nome: "Notebook 14\" fictício — origem 2",
                    descricao: "Equipamento de informática para testes de origem geográfica 2 e origem ICMS 1.",
                    marca: "TechDemo",
                    modelo: "Book14-Mock",
                    gtin: "7891234567890",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: DimensaoProduto.Criar(0.02m, 0.32m, 0.22m, 1.8m, "CM", "KG"),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.08m, 0.38m, 0.28m, 2.2m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("2", "China"),
                    dadosFiscais: DadosFiscais.Criar("84713012", "2108700", "1"),
                    atributosIniciais: new[] { AtributoProduto.Criar("CPU", "Mock i5"), AtributoProduto.Criar("RAM", "8 GB") },
                    skusIniciais: new[] { ("NB-DEMO-14-I5", true), ("NB-DEMO-14-I5-REF", false) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "notebooks"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7891000300025", () =>
                ProdutoEntity.Registrar(
                    nome: "Cadeira flexora — estação Leg Press",
                    descricao: "Estação guiada para extensão de pernas / leg press. Dados ilustrativos (docs/POPOSTA_PRODUTOS.md).",
                    marca: "DeepBlues Fitness",
                    modelo: "dbfit-mextensora-001",
                    gtin: "7891000300025",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: DimensaoProduto.Criar(1.45m, 0.95m, 1.55m, 185m, "CM", "KG"),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(1.60m, 1.00m, 0.55m, 195m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("95069910", "1704400", "0"),
                    atributosIniciais: new[]
                    {
                        AtributoProduto.Criar("Carga máx. indicada", "300 kg (pilha)"),
                        AtributoProduto.Criar("Uso", "Musculação — membros inferiores")
                    },
                    skusIniciais: new[] { ("dbfit-mextensora-001", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "leg-press"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100001", () =>
                ProdutoEntity.Registrar(
                    nome: "Smart TV LED 43\" 4K fictícia",
                    descricao: "Televisor LED para testes de árvore de categorias (Eletrônicos › Vídeo › Televisores › LED).",
                    marca: "VisionDemo",
                    modelo: "LED43-4K",
                    gtin: "7892000100001",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: DimensaoProduto.Criar(0.08m, 0.56m, 0.96m, 9.5m, "CM", "KG"),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.12m, 0.62m, 1.05m, 11.0m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("85287211", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Resolução", "3840×2160 (referência demo)") },
                    skusIniciais: new[] { ("TV-DEMO-LED43", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "led"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100002", () =>
                ProdutoEntity.Registrar(
                    nome: "Soundbar 2.1 canais fictícia",
                    descricao: "Barra de som para demonstração de categoria Áudio › Equipamentos de Áudio › Soundbars.",
                    marca: "SonicBar",
                    modelo: "SB-210",
                    gtin: "7892000100002",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.12m, 0.95m, 0.14m, 3.2m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("85182200", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Potência", "120 W RMS (fictício)") },
                    skusIniciais: new[] { ("AUD-SB210-UN", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "soundbars"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100003", () =>
                ProdutoEntity.Registrar(
                    nome: "Capa TPU transparente smartphone 6,5\"",
                    descricao: "Proteção — Celulares › Acessórios › Proteção › Capas.",
                    marca: "ShieldCase",
                    modelo: "TPU-65",
                    gtin: "7892000100003",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "PCT",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.02m, 0.10m, 0.18m, 0.04m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("39269090", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Compatibilidade", "Smartphones até 6,5\"") },
                    skusIniciais: new[] { ("CEL-CAPA-TPU65", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "capas"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100004", () =>
                ProdutoEntity.Registrar(
                    nome: "Frigideira antiaderente 24 cm",
                    descricao: "Panelas › Antiaderente — utilidades domésticas.",
                    marca: "CozinhaPrática",
                    modelo: "FRG-24",
                    gtin: "7892000100004",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.08m, 0.28m, 0.45m, 0.95m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("76151020", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Revestimento", "Antiaderente cerâmico (demo)") },
                    skusIniciais: new[] { ("UDM-FRG24-AD", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "antiaderente"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100005", () =>
                ProdutoEntity.Registrar(
                    nome: "Roteador Wi-Fi 6 AX1800 fictício",
                    descricao: "Informática › Redes › Roteadores.",
                    marca: "NetWave",
                    modelo: "AX1800-Demo",
                    gtin: "7892000100005",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.08m, 0.22m, 0.32m, 0.55m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("85176259", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Padrão", "Wi-Fi 6 (802.11ax) — fictício") },
                    skusIniciais: new[] { ("NET-AX1800-UN", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "roteadores"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100006", () =>
                ProdutoEntity.Registrar(
                    nome: "Mouse óptico USB — periférico demo",
                    descricao: "Informática › Periféricos › Mouses.",
                    marca: "ClickSoft",
                    modelo: "MO-U100",
                    gtin: "7892000100006",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "BL",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.04m, 0.07m, 0.12m, 0.09m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("84716053", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("DPI", "1600 (referência demo)") },
                    skusIniciais: new[] { ("PER-MOU-U100", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "mouses"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7892000100007", () =>
                ProdutoEntity.Registrar(
                    nome: "Memória RAM DDR4 8 GB 3200 MHz fictícia",
                    descricao: "Componentes › Memória RAM.",
                    marca: "FastMem",
                    modelo: "DDR4-8G3200",
                    gtin: "7892000100007",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "BL",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.03m, 0.14m, 0.04m, 0.02m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("2", "China"),
                    dadosFiscais: DadosFiscais.Criar("85423229", null, "1"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Formato", "DIMM desktop (demo)") },
                    skusIniciais: new[] { ("CMP-RAM-D48G", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "memoria-ram"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutosDemonstracaoFitnessImportadosChina(context, usuarioCarga, mapaCategoria);

            AddProdutosDemonstracaoExtrasPaginacao(context, usuarioCarga, mapaCategoria);

            GarantirCategoriaNosProdutosDemonstracao(context, usuarioCarga, mapaCategoria);

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

    /// <summary>
    /// Atualiza <see cref="ProdutoEntity.CategoriaProdutoId"/> nos GTINs de demonstração quando o produto já existia
    /// (seed anterior à coluna de categoria). Idempotente: só altera quando o vínculo difere ou está nulo.
    /// </summary>
    private static void GarantirCategoriaNosProdutosDemonstracao(
        AppDbContext context,
        string usuarioCarga,
        Dictionary<string, long> mapaCategoria)
    {
        if (mapaCategoria.Count == 0)
            return;

        void Vincular(string gtin, string slug)
        {
            var idCat = IdCategoriaPorSlug(mapaCategoria, slug);
            if (!idCat.HasValue)
                return;

            var p = context.Produtos.FirstOrDefault(x => x.Gtin == gtin);
            if (p is null)
                return;

            if (p.CategoriaProdutoId == idCat.Value)
                return;

            p.AlterarCategoria(idCat, usuarioCarga);
        }

        Vincular("7891234567890", "notebooks");
        Vincular("7891000300025", "leg-press");
        Vincular("7892000100001", "led");
        Vincular("7892000100002", "soundbars");
        Vincular("7892000100003", "capas");
        Vincular("7892000100004", "antiaderente");
        Vincular("7892000100005", "roteadores");
        Vincular("7892000100006", "mouses");
        Vincular("7892000100007", "memoria-ram");

        Vincular("6928365001001", "halteres");
        Vincular("6928365001002", "halteres");
        Vincular("6928365001003", "supino");
        Vincular("6928365001004", "faixas-elasticas");
        Vincular("6928365001005", "acessorios-para-treino-funcional");
        Vincular("6928365001006", "estacoes-de-musculacao");
        Vincular("6928365001007", "kettlebell");
        Vincular("6928365001008", "cordas");
        Vincular("6928365001009", "estacoes-de-musculacao");

        ReadOnlySpan<(string Gtin, string Slug)> extrasPaginacao =
        [
            ("7899010000001", "colchonetes"),
            ("7899010000002", "kettlebell"),
            ("7899010000003", "esteiras"),
            ("7899010000004", "led"),
            ("7899010000005", "mouses"),
            ("7899010000006", "teclados"),
            ("7899010000007", "ssd-e-hd"),
            ("7899010000008", "memoria-ram"),
            ("7899010000009", "roteadores"),
            ("7899010000010", "capas"),
            ("7899010000011", "peliculas"),
            ("7899010000012", "carregadores"),
            ("7899010000013", "power-banks"),
            ("7899010000014", "talheres"),
            ("7899010000015", "antiaderente"),
            ("7899010000016", "caixas-organizadoras"),
            ("7899010000017", "vassouras"),
            ("7899010000018", "mops"),
            ("7899010000019", "varais"),
            ("7899010000020", "halteres"),
        ];

        foreach (var (gtin, slug) in extrasPaginacao)
            Vincular(gtin, slug);
    }

    /// <summary>GTINs 692… (China) — musculação / funcional / yoga; categorias folha conforme árvore nova.</summary>
    private static void AddProdutosDemonstracaoFitnessImportadosChina(
        AppDbContext context,
        string usuarioCarga,
        Dictionary<string, long> mapaCategoria)
    {
        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001001", () =>
            ProdutoEntity.Registrar(
                nome: "Par de halteres hexagonais borracha 10 kg",
                descricao: "Par de halteres revestidos em borracha, pegada antiderrapante. Peso nominal 10 kg por peça. Importado — dados fictícios para demonstração.",
                marca: "PowerSteel CN",
                modelo: "HX-10",
                gtin: "6928365001001",
                unidadeComercializacao: "PAR",
                unidadeMedidaFisica: "KG",
                tipoEmbalagem: "CX",
                dimensaoProduto: DimensaoProduto.Criar(0.16m, 0.30m, 0.16m, 20.5m, "CM", "KG"),
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.20m, 0.35m, 0.20m, 21.0m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069100", null, "1"),
                atributosIniciais: new[]
                {
                    AtributoProduto.Criar("Material", "Ferro fundido com borracha"),
                    AtributoProduto.Criar("Uso", "Musculação / crossfit"),
                },
                skusIniciais: new[] { ("FIT-CN-HX10-PAR", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "halteres"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001002", () =>
            ProdutoEntity.Registrar(
                nome: "Halteres ajustáveis rápidos 2×10 kg",
                descricao: "Par de halteres com sistema de trava rápida, placas ajustáveis até 10 kg por lado. Importado da China — demonstração.",
                marca: "QuickLock",
                modelo: "QL-20",
                gtin: "6928365001002",
                unidadeComercializacao: "PAR",
                unidadeMedidaFisica: "KG",
                tipoEmbalagem: "CX",
                dimensaoProduto: null,
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.45m, 0.25m, 0.18m, 22.0m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069100", null, "1"),
                atributosIniciais: new[] { AtributoProduto.Criar("Peso máx. recomendado", "20 kg total") },
                skusIniciais: new[] { ("FIT-CN-QL20-PAR", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "halteres"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001003", () =>
            ProdutoEntity.Registrar(
                nome: "Banco de supino declinado e reto regulável",
                descricao: "Banco de musculação com encosto e assento ajustáveis em várias posições; estrutura em aço. Importado — uso somente em ambiente demo.",
                marca: "BenchMaster",
                modelo: "BM-500",
                gtin: "6928365001003",
                unidadeComercializacao: "UN",
                unidadeMedidaFisica: "UN",
                tipoEmbalagem: "CX",
                dimensaoProduto: DimensaoProduto.Criar(1.20m, 0.55m, 1.35m, 28.0m, "CM", "KG"),
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(1.25m, 0.60m, 0.45m, 30.0m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069910", null, "1"),
                atributosIniciais: new[]
                {
                    AtributoProduto.Criar("Capacidade indicada", "Até 200 kg (usuário + carga)"),
                    AtributoProduto.Criar("Função", "Peito, costas, ombros"),
                },
                skusIniciais: new[] { ("FIT-CN-BM500-UN", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "supino"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001004", () =>
            ProdutoEntity.Registrar(
                nome: "Kit de elásticos de resistência 11 peças",
                descricao: "Kit com faixas de látex natural, níveis de tensão variados, alças para pés e porta âncora. Importado da China.",
                marca: "FlexBand Pro",
                modelo: "FB-11K",
                gtin: "6928365001004",
                unidadeComercializacao: "KIT",
                unidadeMedidaFisica: "UN",
                tipoEmbalagem: "PCT",
                dimensaoProduto: null,
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.12m, 0.22m, 0.08m, 0.65m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069100", null, "1"),
                atributosIniciais: new[]
                {
                    AtributoProduto.Criar("Conteúdo", "5 faixas + alças + 2 extensores + âncoras"),
                    AtributoProduto.Criar("Treino", "Pilates, funcional, reabilitação"),
                },
                skusIniciais: new[] { ("FIT-CN-FB11-KIT", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "faixas-elasticas"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001005", () =>
            ProdutoEntity.Registrar(
                nome: "Roda de exercício abdominal com apoio para joelhos",
                descricao: "Roda dupla com cabo em borracha e esteira para joelhos; fortalecimento de core. Importado — demonstração.",
                marca: "CoreWheel",
                modelo: "CW-2R",
                gtin: "6928365001005",
                unidadeComercializacao: "UN",
                unidadeMedidaFisica: "UN",
                tipoEmbalagem: "BL",
                dimensaoProduto: null,
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.28m, 0.18m, 0.10m, 0.85m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069100", null, "1"),
                atributosIniciais: new[] { AtributoProduto.Criar("Indicado para", "Abdômen, estabilização") },
                skusIniciais: new[] { ("FIT-CN-CW2R-UN", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "acessorios-para-treino-funcional"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001006", () =>
            ProdutoEntity.Registrar(
                nome: "Aparelho de crunch abdominal com encosto",
                descricao: "Estação compacta para flexão de tronco tipo abdominal com roletes para pés e encosto acolchoado. Importado da China.",
                marca: "AbsLine",
                modelo: "AL-CR",
                gtin: "6928365001006",
                unidadeComercializacao: "UN",
                unidadeMedidaFisica: "UN",
                tipoEmbalagem: "CX",
                dimensaoProduto: DimensaoProduto.Criar(0.95m, 0.48m, 0.78m, 15.0m, "CM", "KG"),
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(1.05m, 0.52m, 0.25m, 16.5m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069910", null, "1"),
                atributosIniciais: new[] { AtributoProduto.Criar("Montagem", "Necessária — manual incluso (fictício)") },
                skusIniciais: new[] { ("FIT-CN-ALCR-UN", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "estacoes-de-musculacao"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001007", () =>
            ProdutoEntity.Registrar(
                nome: "Kettlebell de ferro fundido 12 kg",
                descricao: "Pesa russa com base plana e pegada texturizada. Importado — marca e especificações ilustrativas.",
                marca: "IronKettle CN",
                modelo: "IK-12",
                gtin: "6928365001007",
                unidadeComercializacao: "UN",
                unidadeMedidaFisica: "KG",
                tipoEmbalagem: "CX",
                dimensaoProduto: DimensaoProduto.Criar(0.20m, 0.18m, 0.25m, 12.2m, "CM", "KG"),
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.24m, 0.22m, 0.28m, 12.8m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069100", null, "1"),
                atributosIniciais: new[] { AtributoProduto.Criar("Acabamento", "Pintura eletrostática preta") },
                skusIniciais: new[] { ("FIT-CN-IK12-UN", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "kettlebell"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001008", () =>
            ProdutoEntity.Registrar(
                nome: "Corda de pular speed com rolamento e cabo de aço",
                descricao: "Corda profissional com rolamentos, cabo revestido e cabos ajustáveis. Importado da China.",
                marca: "SpeedRope",
                modelo: "SR-360",
                gtin: "6928365001008",
                unidadeComercializacao: "UN",
                unidadeMedidaFisica: "UN",
                tipoEmbalagem: "PCT",
                dimensaoProduto: null,
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.04m, 0.08m, 0.16m, 0.22m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069100", null, "1"),
                atributosIniciais: new[] { AtributoProduto.Criar("Comprimento", "Ajustável até 3 m") },
                skusIniciais: new[] { ("FIT-CN-SR360-UN", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "cordas"),
                usuarioAuditoria: usuarioCarga));

        AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "6928365001009", () =>
            ProdutoEntity.Registrar(
                nome: "Mini mesa de exercícios multifuncional dobrável",
                descricao: "Apoio inclinado para flexão, prancha e alongamento; estrutura dobrável em aço. Importado — demonstração.",
                marca: "FoldGym",
                modelo: "FG-MINI",
                gtin: "6928365001009",
                unidadeComercializacao: "UN",
                unidadeMedidaFisica: "UN",
                tipoEmbalagem: "CX",
                dimensaoProduto: DimensaoProduto.Criar(0.08m, 0.45m, 0.70m, 8.5m, "CM", "KG"),
                dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.12m, 0.50m, 0.20m, 9.2m, "CM", "KG"),
                origemProduto: OrigemProduto.Criar("2", "China"),
                dadosFiscais: DadosFiscais.Criar("95069910", null, "1"),
                atributosIniciais: new[] { AtributoProduto.Criar("Carga máx. indicada", "120 kg") },
                skusIniciais: new[] { ("FIT-CN-FGMINI-UN", true) },
                categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "estacoes-de-musculacao"),
                usuarioAuditoria: usuarioCarga));
    }

    /// <summary>Vinte itens fictícios (GTINs 7899010000001–20) espalhados na nova árvore — paginação e filtros por categoria.</summary>
    private static void AddProdutosDemonstracaoExtrasPaginacao(
        AppDbContext context,
        string usuarioCarga,
        Dictionary<string, long> mapaCategoria)
    {
        ReadOnlySpan<string> nomes =
        [
            "Colchonete yoga PVC 10 mm",
            "Kettlebell emborrachado 8 kg",
            "Esteira elétrica compacta 1,25 m",
            "Smart TV OLED 55\" fictícia",
            "Mouse sem fio ergonômico",
            "Teclado mecânico ABNT2",
            "SSD NVMe 1 TB fictício",
            "Memória RAM DDR5 16 GB",
            "Switch gerenciável 8 portas",
            "Capa flip couro sintético universal",
            "Película vidro temperado 6,1\"",
            "Carregador USB-C 30 W GaN",
            "Power bank 20.000 mAh",
            "Conjunto talheres inox 24 peças",
            "Panela pressão antiaderente 4,5 L",
            "Caixa organizadora empilhável 15 L",
            "Vassoura pelo sintético cabo longo",
            "Mop spray com reservatório",
            "Varal de chão com abas",
            "Anilha olímpica pintada 20 kg"
        ];

        ReadOnlySpan<string> marcas =
        [
            "ZenMat", "KettleSoft", "RunCompact", "OLEDVision", "ErgoClick",
            "TypeMech", "FlashStore", "FastMem", "NetPort", "CoverLux",
            "GlassShield", "TurboCharge", "AmpBank", "Talheres Sul", "PressCook",
            "Organiza+", "LimpFácil", "MopSpray", "VaralMax", "OlympPlate"
        ];

        ReadOnlySpan<string> ncms =
        [
            "95069910", "95069100", "95069100", "85287212", "84716053",
            "84716011", "85235190", "85423229", "85176241", "39269090",
            "39269090", "85044010", "85044090", "82159900", "76151020",
            "39249000", "96039000", "96039000", "73239900", "95069910"
        ];

        ReadOnlySpan<string> slugsCategoria =
        [
            "colchonetes", "kettlebell", "esteiras", "oled", "mouses",
            "teclados", "ssd-e-hd", "memoria-ram", "switches", "capas",
            "peliculas", "carregadores", "power-banks", "talheres", "antiaderente",
            "caixas-organizadoras", "vassouras", "mops", "varais", "anilhas"
        ];

        for (var i = 0; i < nomes.Length; i++)
        {
            var gtin = $"7899010{(i + 1):D6}";
            var nome = nomes[i];
            var marca = marcas[i];
            var ncm = ncms[i];
            var sku = $"SEED-PAG-{(i + 1):D2}";
            var slug = slugsCategoria[i];

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, gtin, () =>
                ProdutoEntity.Registrar(
                    nome: nome,
                    descricao: "Item fictício para teste de paginação e facetas de categoria no grid administrativo.",
                    marca: marca,
                    modelo: "Demo",
                    gtin: gtin,
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "UN",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.08m, 0.12m, 0.16m, 0.45m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar(ncm, null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Demo", "Paginação / categorias") },
                    skusIniciais: new[] { (sku, true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, slug),
                    usuarioAuditoria: usuarioCarga));
        }
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
            "administracao",
            "execucoesdejobs",
            "Execuções de jobs",
            "Histórico de execuções dos jobs agendados (Quartz).",
            3,
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

        AddPermissaoIfNotExists(context, "execucoesdejobs", "acessar", dataCarga, usuarioCarga);

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
