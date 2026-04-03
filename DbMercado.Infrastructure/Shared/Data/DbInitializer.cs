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

        // ─── Origem ICMS — tabela SEFAZ (códigos 0 a 8) ───
        AddParametroIfNotExists(context, "produto", "origemIcms", "0", "0 — Nacional, exceto códigos 3 a 5",                                null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "1", "1 — Estrangeira, importação direta, exceto código 6",               null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "2", "2 — Estrangeira, adquirida no mercado interno, exceto código 7",    null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "3", "3 — Nacional, conteúdo de importação > 40% e ≤ 70%",               null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "4", "4 — Nacional, processo produtivo básico (PPB)",                    null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "5", "5 — Nacional, conteúdo de importação ≤ 40%",                       null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "6", "6 — Estrangeira, importação direta sem similar nacional",          null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "7", "7 — Estrangeira, mercado interno sem similar nacional",            null, dataCarga, usuarioCarga);
        AddParametroIfNotExists(context, "produto", "origemIcms", "8", "8 — Nacional, conteúdo de importação > 70%",                      null, dataCarga, usuarioCarga);
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
    /// Carga incremental de produtos fictícios (um registro por GTIN, se ainda não existir).
    /// Se as tabelas de produto não existirem (erro 208), registra aviso: aplicar migrações EF (<c>prdProduto</c>).
    /// </summary>
    /// <summary>
    /// Seed incremental de categorias. Retorna dicionário slug → Id para uso no seed de produtos.
    /// </summary>
    private static Dictionary<string, long> AddCategorias(
        AppDbContext context,
        DateTime dataCarga,
        string usuarioCarga)
    {
        var mapa = new Dictionary<string, long>();

        var alimentos = EnsureCategoria(context, "Alimentos", null, dataCarga, usuarioCarga);
        var higieneBeleza = EnsureCategoria(context, "Higiene e Beleza", null, dataCarga, usuarioCarga);
        var eletronicos = EnsureCategoria(context, "Eletrônicos", null, dataCarga, usuarioCarga);
        var utilidades = EnsureCategoria(context, "Utilidades", null, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Fitness", null, dataCarga, usuarioCarga);

        context.SaveChanges();

        var graos = EnsureCategoria(context, "Grãos e Cereais", alimentos.Id, dataCarga, usuarioCarga);
        var oleos = EnsureCategoria(context, "Óleos e Condimentos", alimentos.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Bebidas", alimentos.Id, dataCarga, usuarioCarga);
        var cafe = EnsureCategoria(context, "Café e Derivados", alimentos.Id, dataCarga, usuarioCarga);
        var limpeza = EnsureCategoria(context, "Limpeza", utilidades.Id, dataCarga, usuarioCarga);
        _ = EnsureCategoria(context, "Cuidado Pessoal", higieneBeleza.Id, dataCarga, usuarioCarga);
        var informatica = EnsureCategoria(context, "Informática", eletronicos.Id, dataCarga, usuarioCarga);

        context.SaveChanges();

        var arroz = EnsureCategoria(context, "Arroz", graos.Id, dataCarga, usuarioCarga);
        var azeites = EnsureCategoria(context, "Azeites", oleos.Id, dataCarga, usuarioCarga);
        var detergentes = EnsureCategoria(context, "Detergentes", limpeza.Id, dataCarga, usuarioCarga);
        var notebooks = EnsureCategoria(context, "Notebooks", informatica.Id, dataCarga, usuarioCarga);
        var cafesTorrados = EnsureCategoria(context, "Cafés Torrados", cafe.Id, dataCarga, usuarioCarga);

        context.SaveChanges();

        EnsureCategoria(context, "Parboilizado", arroz.Id, dataCarga, usuarioCarga);
        EnsureCategoria(context, "Extra Virgem", azeites.Id, dataCarga, usuarioCarga);
        EnsureCategoria(context, "Multiuso", detergentes.Id, dataCarga, usuarioCarga);
        EnsureCategoria(context, "Ultrafinos", notebooks.Id, dataCarga, usuarioCarga);
        EnsureCategoria(context, "Em Grãos", cafesTorrados.Id, dataCarga, usuarioCarga);

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

    private static void AddProdutosDemonstracao(AppDbContext context, string usuarioCarga, ILogger logger, Dictionary<string, long> mapaCategoria)
    {
        try
        {
            CorrigirNomesProdutosComPrefixoLegadoSeed(context, logger);

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7893500030518", () =>
                ProdutoEntity.Registrar(
                    nome: "Arroz parboilizado Tio João 1 kg",
                    descricao: "Arroz longo fino tipo 1, embalagem plástica. Dados ilustrativos para ambiente de demonstração.",
                    marca: "Tio João",
                    modelo: "Tipo 1",
                    gtin: "7893500030518",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "KG",
                    tipoEmbalagem: "PCT",
                    dimensaoProduto: DimensaoProduto.Criar(0.04m, 0.15m, 0.22m, 1.0m, "CM", "KG"),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.045m, 0.16m, 0.23m, 1.05m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("10063021", "1705500", "0"),
                    atributosIniciais: new[]
                    {
                        AtributoProduto.Criar("Armazenamento", "Local seco e arejado"),
                        AtributoProduto.Criar("Validade típica", "12 meses (referência fictícia)")
                    },
                    skusIniciais: new[] { ("ARZ-TJ-1KG-UN", true), ("ARZ-TJ-1KG-CX12", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "parboilizado"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7896048320065", () =>
                ProdutoEntity.Registrar(
                    nome: "Azeite extra virgem Andorinha 500 ml",
                    descricao: "Azeite de oliva extra virgem, vidro. Demonstração — não é oferta comercial.",
                    marca: "Andorinha",
                    modelo: "Extra virgem",
                    gtin: "7896048320065",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "ML",
                    tipoEmbalagem: "FR",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.22m, 0.07m, 0.07m, 0.85m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("15091000", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Volume", "500 ml") },
                    skusIniciais: new[] { ("AZE-AND-500ML", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "extra-virgem"),
                    usuarioAuditoria: usuarioCarga));

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
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "ultrafinos"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7891000100103", () =>
                ProdutoEntity.Registrar(
                    nome: "Detergente líquido limpeza total 500 ml",
                    descricao: "Agente de limpeza — caixa com múltiplas unidades (SKU principal por frasco).",
                    marca: "LimpaBem",
                    modelo: "Neutro",
                    gtin: "7891000100103",
                    unidadeComercializacao: "CX",
                    unidadeMedidaFisica: "L",
                    tipoEmbalagem: "CX",
                    dimensaoProduto: null,
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.25m, 0.32m, 0.40m, 6.5m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("34022000", "2803800", "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Fragrância", "Limão"), AtributoProduto.Criar("pH", "~7") },
                    skusIniciais: new[] { ("DET-LIM-500-CX24", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "multiuso"),
                    usuarioAuditoria: usuarioCarga));

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, "7896004001234", () =>
                ProdutoEntity.Registrar(
                    nome: "Café torrado em grãos especial 250 g",
                    descricao: "Café arábica torrado, acondicionado a vácuo. Peso líquido 250 g (unidade de venda: pacote).",
                    marca: "Café do Cerrado",
                    modelo: "Grãos inteiros",
                    gtin: "7896004001234",
                    unidadeComercializacao: "UN",
                    unidadeMedidaFisica: "KG",
                    tipoEmbalagem: "PCT",
                    dimensaoProduto: DimensaoProduto.Criar(0.03m, 0.12m, 0.18m, 0.25m, "CM", "KG"),
                    dimensaoEmbalagem: DimensaoEmbalagem.Criar(0.035m, 0.13m, 0.19m, 0.26m, "CM", "KG"),
                    origemProduto: OrigemProduto.Criar("1", null),
                    dadosFiscais: DadosFiscais.Criar("09011100", null, "0"),
                    atributosIniciais: new[] { AtributoProduto.Criar("Torra", "Média"), AtributoProduto.Criar("Safra", "Referência demo") },
                    skusIniciais: new[] { ("CAF-CER-250G", true), ("CAF-CER-250G-ORG", true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "em-graos"),
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

        Vincular("7893500030518", "parboilizado");
        Vincular("7896048320065", "extra-virgem");
        Vincular("7891234567890", "ultrafinos");
        Vincular("7891000100103", "multiuso");
        Vincular("7896004001234", "em-graos");

        foreach (var gtin in GtinsProdutosFitnessImportadosChina)
            Vincular(gtin, "fitness");

        for (var i = 0; i < 20; i++)
            Vincular($"7899010{(i + 1):D6}", "graos-e-cereais");
    }

    /// <summary>GTINs 692… (prefixo comercial China) — seed importados para filtro de origem.</summary>
    private static readonly string[] GtinsProdutosFitnessImportadosChina =
    [
        "6928365001001",
        "6928365001002",
        "6928365001003",
        "6928365001004",
        "6928365001005",
        "6928365001006",
        "6928365001007",
        "6928365001008",
        "6928365001009",
    ];

    /// <summary>Equipamentos de musculação e aeróbico — origem China (demonstração).</summary>
    private static void AddProdutosDemonstracaoFitnessImportadosChina(
        AppDbContext context,
        string usuarioCarga,
        Dictionary<string, long> mapaCategoria)
    {
        var idFitness = IdCategoriaPorSlug(mapaCategoria, "fitness");
        if (!idFitness.HasValue)
            return;

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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
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
                categoriaProdutoId: idFitness,
                usuarioAuditoria: usuarioCarga));
    }

    /// <summary>Vinte itens fictícios adicionais (GTINs 7899010000001–20) para exercitar paginação do grid.</summary>
    private static void AddProdutosDemonstracaoExtrasPaginacao(
        AppDbContext context,
        string usuarioCarga,
        Dictionary<string, long> mapaCategoria)
    {
        ReadOnlySpan<string> nomes =
        [
            "Leite integral UHT 1 L",
            "Açúcar cristal 1 kg",
            "Farinha de trigo tipo 1 1 kg",
            "Óleo de soja 900 ml",
            "Macarrão espaguete 500 g",
            "Molho de tomate tradicional 340 g",
            "Feijão preto tipo 1 1 kg",
            "Sal refinado iodado 1 kg",
            "Vinagre de álcool 750 ml",
            "Achocolatado em pó 400 g",
            "Biscoito cream cracker 400 g",
            "Sardinha em lata 125 g",
            "Atum em conserva 170 g",
            "Suco de laranja integral 1 L",
            "Iogurte natural 170 g",
            "Queijo minas frescal porção",
            "Manteiga com sal 200 g",
            "Papel higiênico folha dupla 30 m",
            "Sabonete líquido 250 ml",
            "Shampoo hidratante 350 ml"
        ];

        ReadOnlySpan<string> marcas =
        [
            "Lácteos Demo", "DoceVida", "Moinho Norte", "Soja Mais", "Massas Itália",
            "Tomate Feliz", "Grãos do Sertão", "Sal do Mar", "Vinagreira", "Chocolate Kids",
            "Snack Bom", "Peixe Azul", "ConservaFit", "Citros", "Iogurte Vivo",
            "Queijos Mineiros", "Manteiga Ouro", "HigieneSoft", "Limpeza Total", "Cabelos Lindos"
        ];

        ReadOnlySpan<string> ncms =
        [
            "04012010", "17019900", "11010010", "15079011", "19021100",
            "20021000", "07133319", "25010011", "22090000", "18069000",
            "19053100", "16041311", "16041410", "20091200", "04039000",
            "04061010", "04051000", "48181000", "34013000", "33051000"
        ];

        for (var i = 0; i < nomes.Length; i++)
        {
            var gtin = $"7899010{(i + 1):D6}";
            var nome = nomes[i];
            var marca = marcas[i];
            var ncm = ncms[i];
            var sku = $"SEED-PAG-{(i + 1):D2}";

            AddProdutoDemonstracaoIfNotExistsPorGtin(context, usuarioCarga, gtin, () =>
                ProdutoEntity.Registrar(
                    nome: nome,
                    descricao: "Item fictício para teste de paginação no grid administrativo.",
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
                    atributosIniciais: new[] { AtributoProduto.Criar("Demo", "Paginação grid") },
                    skusIniciais: new[] { (sku, true) },
                    categoriaProdutoId: IdCategoriaPorSlug(mapaCategoria, "graos-e-cereais"),
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
