using DbMercado.Domain.Produto.ValueObjects;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.Entities;

/// <summary>
/// Raiz de agregado de produto com foco logístico e fiscal.
/// </summary>
public class ProdutoEntity : BaseEntity
{
    public long Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public string? Marca { get; private set; }

    public string? Modelo { get; private set; }

    public string? Gtin { get; private set; }

    /// <summary>Categoria do produto na árvore de categorias (opcional na transição).</summary>
    public long? CategoriaProdutoId { get; private set; }

    public CategoriaProdutoEntity? CategoriaProduto { get; private set; }

    /// <summary>Como o produto é vendido/faturado (código do parâmetro: UN, KIT, DZ, CX...).</summary>
    public string UnidadeComercializacao { get; private set; } = string.Empty;

    /// <summary>Natureza física para NF-e (código do parâmetro: UN, KG, L, M...).</summary>
    public string UnidadeMedidaFisica { get; private set; } = string.Empty;

    /// <summary>Tipo de acondicionamento da embalagem (código do parâmetro: CX, FD, PCT, LAT...).</summary>
    public string TipoEmbalagem { get; private set; } = string.Empty;

    public DimensaoProduto? DimensaoProduto { get; private set; }

    public DimensaoEmbalagem DimensaoEmbalagem { get; private set; } = null!;

    public OrigemProduto OrigemProduto { get; private set; } = null!;

    public DadosFiscais DadosFiscais { get; private set; } = null!;

    /// <summary>SKUs do agregado (coleção rastreada pelo ORM).</summary>
    public ICollection<SkuEntity> Skus { get; private set; } = new List<SkuEntity>();

    /// <summary>Atributos flexíveis (owned collection no ORM).</summary>
    public ICollection<AtributoProduto> Atributos { get; private set; } = new List<AtributoProduto>();

    public ProdutoEntity()
    {
    }

    /// <summary>
    /// Registra um novo produto com pelo menos um SKU e invariantes fiscais/logísticas.
    /// </summary>
    public static ProdutoEntity Registrar(
        string nome,
        string? descricao,
        string? marca,
        string? modelo,
        string? gtin,
        string unidadeComercializacao,
        string unidadeMedidaFisica,
        string tipoEmbalagem,
        DimensaoProduto? dimensaoProduto,
        DimensaoEmbalagem dimensaoEmbalagem,
        OrigemProduto origemProduto,
        DadosFiscais dadosFiscais,
        IEnumerable<AtributoProduto>? atributosIniciais,
        IReadOnlyCollection<(string Codigo, bool Ativo)> skusIniciais,
        long? categoriaProdutoId,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(dimensaoEmbalagem);
        ArgumentNullException.ThrowIfNull(origemProduto);
        ArgumentNullException.ThrowIfNull(dadosFiscais);
        ArgumentNullException.ThrowIfNull(skusIniciais);

        if (skusIniciais.Count == 0)
            throw new BusinessException("PRODUTO_SKU_MINIMO", "O produto deve possuir pelo menos um SKU.");

        var agora = DateTime.UtcNow;
        var entidade = new ProdutoEntity
        {
            Nome = nome?.Trim() ?? string.Empty,
            Descricao = descricao?.Trim() ?? string.Empty,
            Marca = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim(),
            Modelo = string.IsNullOrWhiteSpace(modelo) ? null : modelo.Trim(),
            Gtin = string.IsNullOrWhiteSpace(gtin) ? null : gtin.Trim(),
            CategoriaProdutoId = categoriaProdutoId,
            UnidadeComercializacao = ValidarCodigoUnidade(unidadeComercializacao, "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA", "Unidade de comercialização é obrigatória."),
            UnidadeMedidaFisica = ValidarCodigoUnidade(unidadeMedidaFisica, "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA", "Unidade de medida física é obrigatória."),
            TipoEmbalagem = ValidarCodigoUnidade(tipoEmbalagem, "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO", "Tipo de embalagem é obrigatório."),
            DimensaoProduto = dimensaoProduto,
            DimensaoEmbalagem = dimensaoEmbalagem,
            OrigemProduto = origemProduto,
            DadosFiscais = dadosFiscais,
            DataCriacao = agora,
            DataUltimaAlteracao = agora,
            UsuarioCriacao = usuarioAuditoria,
            UsuarioUltimaAlteracao = usuarioAuditoria
        };

        if (atributosIniciais is not null)
        {
            foreach (var a in atributosIniciais)
                entidade.Atributos.Add(a);
        }

        GarantirNomeInformado(entidade.Nome);
        GarantirSkusSemDuplicidade(skusIniciais.Select(s => s.Codigo));

        foreach (var (codigo, ativo) in skusIniciais)
        {
            var sku = SkuEntity.CriarNovaEntrada(entidade, codigo, ativo, usuarioAuditoria);
            entidade.Skus.Add(sku);
        }

        entidade.Validar();
        return entidade;
    }

    public void AdicionarSku(string codigo, bool ativo, string usuarioAuditoria)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new BusinessException("SKU_CODIGO_OBRIGATORIO", "Código do SKU é obrigatório.");

        if (SkuComCodigoExiste(codigo))
            throw new DuplicateEntityException("PRODUTO_SKU_DUPLICADO", $"Já existe SKU com o código '{codigo.Trim()}'.")
                .With("Codigo", codigo.Trim());

        var sku = SkuEntity.CriarNovaEntrada(this, codigo, ativo, usuarioAuditoria);
        Skus.Add(sku);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    public void RemoverSku(string codigoSku, string usuarioAuditoria)
    {
        if (Skus.Count <= 1)
            throw new BusinessException("PRODUTO_SKU_MINIMO", "Não é possível remover o SKU: o produto deve manter pelo menos um SKU.");

        if (string.IsNullOrWhiteSpace(codigoSku))
            throw new BusinessException("SKU_CODIGO_OBRIGATORIO", "Código do SKU é obrigatório para remoção.");

        var normalizado = codigoSku.Trim();
        var sku = Skus.FirstOrDefault(s => string.Equals(s.Codigo, normalizado, StringComparison.OrdinalIgnoreCase));
        if (sku is null)
            throw new BusinessException("PRODUTO_SKU_NAO_ENCONTRADO", "SKU não encontrado neste produto.")
                .With("CodigoSku", normalizado);

        Skus.Remove(sku);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    public void AtualizarDadosBasicos(
        string nome,
        string? descricao,
        string? marca,
        string? modelo,
        string? gtin,
        string unidadeComercializacao,
        string unidadeMedidaFisica,
        string tipoEmbalagem,
        OrigemProduto origemProduto,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(origemProduto);

        Nome = nome?.Trim() ?? string.Empty;
        Descricao = descricao?.Trim() ?? string.Empty;
        Marca = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim();
        Modelo = string.IsNullOrWhiteSpace(modelo) ? null : modelo.Trim();
        Gtin = string.IsNullOrWhiteSpace(gtin) ? null : gtin.Trim();
        UnidadeComercializacao = ValidarCodigoUnidade(unidadeComercializacao, "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA", "Unidade de comercialização é obrigatória.");
        UnidadeMedidaFisica = ValidarCodigoUnidade(unidadeMedidaFisica, "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA", "Unidade de medida física é obrigatória.");
        TipoEmbalagem = ValidarCodigoUnidade(tipoEmbalagem, "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO", "Tipo de embalagem é obrigatório.");
        OrigemProduto = origemProduto;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    /// <summary>
    /// Alinha a coleção de SKUs à lista desejada (adições, remoções e flag <see cref="SkuEntity.Ativo"/>).
    /// </summary>
    public void SincronizarSkus(IReadOnlyCollection<(string Codigo, bool Ativo)> desejados, string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(desejados);

        if (desejados.Count == 0)
            throw new BusinessException("PRODUTO_SKU_MINIMO", "Informe pelo menos um SKU.");

        var norm = desejados.Select(d => (Codigo: d.Codigo.Trim(), d.Ativo)).ToList();
        GarantirSkusSemDuplicidade(norm.Select(x => x.Codigo));

        var setDesejado = new HashSet<string>(norm.Select(x => x.Codigo), StringComparer.OrdinalIgnoreCase);

        foreach (var (codigo, ativo) in norm)
        {
            if (!SkuComCodigoExiste(codigo))
                AdicionarSku(codigo, ativo, usuarioAuditoria);
        }

        foreach (var (codigo, ativo) in norm)
        {
            var sku = Skus.FirstOrDefault(s => string.Equals(s.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
            sku?.AlterarAtivo(ativo, usuarioAuditoria);
        }

        foreach (var sku in Skus.Where(s => !setDesejado.Contains(s.Codigo)).ToList())
            RemoverSku(sku.Codigo, usuarioAuditoria);

        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    public void AtualizarDimensoes(
        DimensaoProduto? dimensaoProduto,
        DimensaoEmbalagem dimensaoEmbalagem,
        string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(dimensaoEmbalagem);

        DimensaoProduto = dimensaoProduto;
        DimensaoEmbalagem = dimensaoEmbalagem;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    public void AtualizarDadosFiscais(DadosFiscais dadosFiscais, string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(dadosFiscais);

        DadosFiscais = dadosFiscais;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    public void AlterarCategoria(long? categoriaId, string usuarioAuditoria)
    {
        CategoriaProdutoId = categoriaId;
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
    }

    /// <summary>
    /// Substitui a coleção de atributos flexíveis do produto (nome/valor).
    /// </summary>
    public void SubstituirAtributos(IEnumerable<AtributoProduto> atributos, string usuarioAuditoria)
    {
        ArgumentNullException.ThrowIfNull(atributos);

        Atributos.Clear();
        foreach (var a in atributos)
            Atributos.Add(a);
        RegistrarAuditoriaAlteracao(usuarioAuditoria);
        Validar();
    }

    /// <summary>
    /// Revalida todas as regras de domínio do agregado.
    /// </summary>
    public void Validar()
    {
        GarantirNomeInformado(Nome);
        ValidarCodigoUnidade(UnidadeComercializacao, "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA", "Unidade de comercialização é obrigatória.");
        ValidarCodigoUnidade(UnidadeMedidaFisica, "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA", "Unidade de medida física é obrigatória.");
        ValidarCodigoUnidade(TipoEmbalagem, "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO", "Tipo de embalagem é obrigatório.");
        GarantirEmbalagemInformada(DimensaoEmbalagem);
        DimensaoEmbalagem.GarantirInvariantes();
        DimensaoProduto?.GarantirInvariantes();
        GarantirOrigemInformada(OrigemProduto);
        GarantirDadosFiscaisInformados(DadosFiscais);

        if (Skus.Count == 0)
            throw new BusinessException("PRODUTO_SKU_MINIMO", "O produto deve possuir pelo menos um SKU.");

        GarantirSkusSemDuplicidade(Skus.Select(s => s.Codigo));
    }

    private static string ValidarCodigoUnidade(string codigo, string errorCode, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new BusinessException(errorCode, errorMessage);

        var c = codigo.Trim().ToUpperInvariant();
        if (c.Length == 0 || c.Length > 16)
            throw new BusinessException(errorCode, errorMessage).With("CodigoInformado", codigo);

        foreach (var ch in c.AsSpan())
        {
            if (!char.IsLetterOrDigit(ch))
                throw new BusinessException(errorCode, errorMessage).With("CodigoInformado", codigo);
        }

        return c;
    }

    private static void GarantirNomeInformado(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessException("PRODUTO_NOME_OBRIGATORIO", "Nome do produto é obrigatório.");
    }

    private static void GarantirEmbalagemInformada(DimensaoEmbalagem? embalagem)
    {
        if (embalagem is null)
            throw new BusinessException("PRODUTO_EMBALAGEM_OBRIGATORIA", "Dimensão da embalagem é obrigatória.");
    }

    private static void GarantirOrigemInformada(OrigemProduto? origem)
    {
        if (origem is null)
            throw new BusinessException("PRODUTO_ORIGEM_OBRIGATORIA", "Origem do produto é obrigatória.");
    }

    private static void GarantirDadosFiscaisInformados(DadosFiscais? dados)
    {
        if (dados is null)
            throw new BusinessException("PRODUTO_DADOS_FISCAIS_OBRIGATORIOS", "Dados fiscais do produto são obrigatórios.");

        dados.GarantirInvariantes();
    }

    private static void GarantirSkusSemDuplicidade(IEnumerable<string> codigos)
    {
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var codigo in codigos)
        {
            var c = codigo.Trim();
            if (!vistos.Add(c))
                throw new DuplicateEntityException("PRODUTO_SKU_DUPLICADO",
                        $"Existem SKUs com o mesmo código '{c}' (a comparação ignora maiúsculas e minúsculas). Cada item precisa de um código único.")
                    .With("Codigo", c);
        }
    }

    private bool SkuComCodigoExiste(string codigo)
    {
        var c = codigo.Trim();
        return Skus.Any(s => string.Equals(s.Codigo, c, StringComparison.OrdinalIgnoreCase));
    }

    private void RegistrarAuditoriaAlteracao(string usuarioAuditoria)
    {
        DataUltimaAlteracao = DateTime.UtcNow;
        UsuarioUltimaAlteracao = usuarioAuditoria;
    }
}
