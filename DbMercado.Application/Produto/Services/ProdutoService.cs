using DbMercado.Application.Produto.Dtos;
using DbMercado.Application.Produto.Interfaces;
using DbMercado.Application.Produto.Mapping;
using System.Text;
using DbMercado.Domain.Administracao.Interfaces.Repositories;
using DbMercado.Domain.Produto.Entities;
using DbMercado.Domain.Produto.Interfaces.UnitsOfWork;
using DbMercado.Domain.Produto.Queries;
using DbMercado.Domain.Produto.ValueObjects;
using DbMercado.Domain.Shared.Exceptions;
using System.Globalization;

namespace DbMercado.Application.Produto.Services;

public class ProdutoService : IProdutoService
{
    private const string DicaParametrosCategoriaProduto =
        " Verifique os cadastros de parâmetros na categoria de produto (administração do sistema).";

    private readonly IUwProduto _uw;
    private readonly IParametroChaveConsultaRepository _parametrosConsulta;
    private readonly IMidiaService _midiaService;

    public ProdutoService(
        IUwProduto uw,
        IParametroChaveConsultaRepository parametrosConsulta,
        IMidiaService midiaService)
    {
        _uw = uw;
        _parametrosConsulta = parametrosConsulta;
        _midiaService = midiaService;
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesComercializacaoAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoUnidadeComercializacao,
            cancellationToken);
        return itens.Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor)).ToList();
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesMedidaAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoUnidadeMedida,
            cancellationToken);
        return itens.Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor)).ToList();
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarTiposEmbalagemAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoUnidadeEmbalagem,
            cancellationToken);
        return itens.Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor)).ToList();
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesDimensaoAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoUnidadeDimensao,
            cancellationToken);
        return itens.Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor)).ToList();
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarUnidadesPesoAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoUnidadePeso,
            cancellationToken);
        return itens.Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor)).ToList();
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarOrigensGeograficasAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoOrigemGeografica,
            cancellationToken);
        return itens.Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor)).ToList();
    }

    public async Task<IReadOnlyList<ProdutoUnidadeMedidaOpcaoDto>> ListarOrigensIcmsAsync(
        CancellationToken cancellationToken = default)
    {
        var itens = await _parametrosConsulta.ListarPorCategoriaEAtributoAsync(
            ProdutoParametrosCatalogo.Categoria,
            ProdutoParametrosCatalogo.AtributoOrigemIcms,
            cancellationToken);

        // Uma opção por código (0–8). O seed antigo usava unicidade Chave+Valor, permitindo duas linhas com a mesma Chave.
        static int OrdemChaveNumerica(string chave) =>
            int.TryParse(chave.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
                ? n
                : int.MaxValue;

        var dedup = itens
            .GroupBy(i => i.Chave.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderBy(x => x.Valor, StringComparer.Ordinal).First())
            .OrderBy(x => OrdemChaveNumerica(x.Chave))
            .ThenBy(x => x.Chave, StringComparer.Ordinal)
            .Select(i => new ProdutoUnidadeMedidaOpcaoDto(i.Chave, i.Valor))
            .ToList();

        return dedup;
    }

    public async Task<long> CriarProdutoAsync(string usuarioAutenticado, ProdutoCreateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.OrigemGeografica);
        ArgumentNullException.ThrowIfNull(dto.DadosFiscais);
        ArgumentNullException.ThrowIfNull(dto.DimensaoEmbalagem);
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioAutenticado);

        await ValidarCatalogoProdutoAsync(dto, cancellationToken);

        var origem = OrigemProduto.Criar(dto.OrigemGeografica.Tipo, dto.OrigemGeografica.PaisOrigem);
        var dadosFiscais = DadosFiscais.Criar(dto.DadosFiscais.Ncm, dto.DadosFiscais.Cest, dto.DadosFiscais.Origem);
        var embalagem = DimensaoEmbalagem.Criar(
            dto.DimensaoEmbalagem.Altura,
            dto.DimensaoEmbalagem.Largura,
            dto.DimensaoEmbalagem.Comprimento,
            dto.DimensaoEmbalagem.Peso,
            dto.DimensaoEmbalagem.UnidadeDimensao,
            dto.DimensaoEmbalagem.UnidadePeso);
        DimensaoProduto? dimProduto = null;
        if (dto.DimensaoProduto is not null)
        {
            dimProduto = DimensaoProduto.Criar(
                dto.DimensaoProduto.Altura,
                dto.DimensaoProduto.Largura,
                dto.DimensaoProduto.Comprimento,
                dto.DimensaoProduto.Peso,
                dto.DimensaoProduto.UnidadeDimensao,
                dto.DimensaoProduto.UnidadePeso);
        }

        IEnumerable<AtributoProduto>? atributos = null;
        if (dto.Atributos is { Count: > 0 })
            atributos = dto.Atributos.Select(a => AtributoProduto.Criar(a.Nome, a.Valor));

        var skus = dto.Skus.Select(s => (s.Codigo, s.Ativo)).ToList();
        if (skus.Count == 0)
            throw new BusinessException("PRODUTO_SKU_MINIMO", "Informe pelo menos um SKU.");

        var entidade = ProdutoEntity.Registrar(
            dto.Nome,
            dto.Descricao,
            dto.Marca,
            dto.Modelo,
            dto.Gtin,
            dto.UnidadeComercializacao,
            dto.UnidadeMedidaFisica,
            dto.TipoEmbalagem,
            dimProduto,
            embalagem,
            origem,
            dadosFiscais,
            atributos,
            skus,
            dto.CategoriaProdutoId,
            usuarioAutenticado);

        await _uw.ProdutoRepository.AddAsync(usuarioAutenticado, entidade);
        await _uw.SaveChangesAsync(cancellationToken);

        var novoId = entidade.Id;
        if (dto.Midias is { Count: > 0 })
        {
            await _midiaService.AssociarAoProdutoAsync(
                novoId,
                new MidiaAssociarDto { Itens = dto.Midias.ToList() },
                usuarioAutenticado,
                cancellationToken);
        }

        return novoId;
    }

    public async Task AtualizarProdutoAsync(string usuarioAutenticado, long id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.OrigemGeografica);
        ArgumentNullException.ThrowIfNull(dto.DadosFiscais);
        ArgumentNullException.ThrowIfNull(dto.DimensaoEmbalagem);
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioAutenticado);

        await ValidarCatalogoProdutoAsync(dto, cancellationToken);

        var entidade = await _uw.ProdutoRepository.GetByIdCompletoAsync(id, rastrear: true, cancellationToken);
        if (entidade is null)
            throw new EntityNotFoundException(nameof(ProdutoEntity), id);

        var origem = OrigemProduto.Criar(dto.OrigemGeografica.Tipo, dto.OrigemGeografica.PaisOrigem);
        entidade.AtualizarDadosBasicos(
            dto.Nome,
            dto.Descricao,
            dto.Marca,
            dto.Modelo,
            dto.Gtin,
            dto.UnidadeComercializacao,
            dto.UnidadeMedidaFisica,
            dto.TipoEmbalagem,
            origem,
            usuarioAutenticado);

        DimensaoProduto? dimProduto = null;
        if (dto.DimensaoProduto is not null)
        {
            dimProduto = DimensaoProduto.Criar(
                dto.DimensaoProduto.Altura,
                dto.DimensaoProduto.Largura,
                dto.DimensaoProduto.Comprimento,
                dto.DimensaoProduto.Peso,
                dto.DimensaoProduto.UnidadeDimensao,
                dto.DimensaoProduto.UnidadePeso);
        }

        var embalagem = DimensaoEmbalagem.Criar(
            dto.DimensaoEmbalagem.Altura,
            dto.DimensaoEmbalagem.Largura,
            dto.DimensaoEmbalagem.Comprimento,
            dto.DimensaoEmbalagem.Peso,
            dto.DimensaoEmbalagem.UnidadeDimensao,
            dto.DimensaoEmbalagem.UnidadePeso);
        entidade.AtualizarDimensoes(dimProduto, embalagem, usuarioAutenticado);

        var dadosFiscais = DadosFiscais.Criar(dto.DadosFiscais.Ncm, dto.DadosFiscais.Cest, dto.DadosFiscais.Origem);
        entidade.AtualizarDadosFiscais(dadosFiscais, usuarioAutenticado);

        var atributosLista = dto.Atributos?.Select(a => AtributoProduto.Criar(a.Nome, a.Valor)).ToList() ?? new List<AtributoProduto>();
        entidade.SubstituirAtributos(atributosLista, usuarioAutenticado);

        var skus = dto.Skus.Select(s => (s.Codigo, s.Ativo)).ToList();
        entidade.SincronizarSkus(skus, usuarioAutenticado);

        entidade.AlterarCategoria(dto.CategoriaProdutoId, usuarioAutenticado);

        await _uw.SaveChangesAsync(cancellationToken);

        if (dto.Midias is { Count: > 0 })
        {
            await _midiaService.AssociarAoProdutoAsync(
                id,
                new MidiaAssociarDto { Itens = dto.Midias.ToList() },
                usuarioAutenticado,
                cancellationToken);
        }
    }

    public async Task ExcluirProdutoAsync(string usuarioAutenticado, long id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioAutenticado);

        var entidade = await _uw.ProdutoRepository.GetByIdCompletoAsync(id, rastrear: true, cancellationToken);
        if (entidade is null)
            throw new EntityNotFoundException(nameof(ProdutoEntity), id);

        await _uw.ProdutoRepository.DeleteAsync(entidade);
        await _uw.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProdutoResponseDto?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entidade = await _uw.ProdutoRepository.GetByIdCompletoAsync(id, rastrear: false, cancellationToken);
        return entidade is null ? null : MapearResposta(entidade);
    }

    public async Task<ProdutoLogisticaResponseDto?> ObterLogisticaAsync(long id, CancellationToken cancellationToken = default)
    {
        var entidade = await _uw.ProdutoRepository.GetByIdCompletoAsync(id, rastrear: false, cancellationToken);
        if (entidade is null)
            return null;

        ProdutoDimensaoDto? dimP = null;
        if (entidade.DimensaoProduto is not null)
        {
            dimP = new ProdutoDimensaoDto
            {
                Altura = entidade.DimensaoProduto.Altura,
                Largura = entidade.DimensaoProduto.Largura,
                Comprimento = entidade.DimensaoProduto.Comprimento,
                Peso = entidade.DimensaoProduto.Peso,
                UnidadeDimensao = entidade.DimensaoProduto.UnidadeDimensao,
                UnidadePeso = entidade.DimensaoProduto.UnidadePeso
            };
        }

        return new ProdutoLogisticaResponseDto
        {
            DimensaoProduto = dimP,
            DimensaoEmbalagem = new ProdutoDimensaoEmbalagemDto
            {
                Altura = entidade.DimensaoEmbalagem.Altura,
                Largura = entidade.DimensaoEmbalagem.Largura,
                Comprimento = entidade.DimensaoEmbalagem.Comprimento,
                Peso = entidade.DimensaoEmbalagem.Peso,
                UnidadeDimensao = entidade.DimensaoEmbalagem.UnidadeDimensao,
                UnidadePeso = entidade.DimensaoEmbalagem.UnidadePeso
            },
            UnidadeComercializacao = entidade.UnidadeComercializacao,
            UnidadeMedidaFisica = entidade.UnidadeMedidaFisica,
            TipoEmbalagem = entidade.TipoEmbalagem
        };
    }

    public async Task<IReadOnlyList<ProdutoResumoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var lista = await _uw.ProdutoRepository.ListarCatalogoAsync(cancellationToken);
        return lista
            .Select(p => new ProdutoResumoDto
            {
                Id = p.Id,
                Nome = p.Nome,
                UnidadeComercializacao = p.UnidadeComercializacao,
                UnidadeMedidaFisica = p.UnidadeMedidaFisica,
                TipoEmbalagem = p.TipoEmbalagem,
                Marca = p.Marca
            })
            .ToList();
    }

    public async Task<ProdutoGridResultDto> ConsultarGridAsync(
        ProdutoGridQueryDto query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var spec = ProdutoGridQueryMapper.ToSpecification(query);
        var (linhas, total) = await _uw.ProdutoRepository.ConsultarGridAsync(spec, cancellationToken);
        return new ProdutoGridResultDto
        {
            RowCount = total,
            Rows = linhas.Select(l => MapearLinhaGrid(l, spec.AgregarContagemId)).ToList()
        };
    }

    private static ProdutoGridRowDto MapearLinhaGrid(
        ProdutoGridLinhaConsulta linha,
        bool agregarContagemId)
    {
        if (linha.LinhaDeGrupo)
        {
            linha.ValoresAgrupamento.TryGetValue("marca", out var marcaRaw);
            var marca = string.IsNullOrEmpty(marcaRaw) ? null : marcaRaw;
            var uFis = linha.ValoresAgrupamento.GetValueOrDefault("unidadeMedida") ?? string.Empty;
            return new ProdutoGridRowDto
            {
                IsGroup = true,
                Id = agregarContagemId ? linha.ContagemFilhosDiretos : null,
                Nome = linha.ValoresAgrupamento.GetValueOrDefault("nome") ?? string.Empty,
                UnidadeComercializacao = string.Empty,
                UnidadeMedidaFisica = uFis,
                TipoEmbalagem = string.Empty,
                Marca = marca,
                CategoriaNome = null,
                CategoriaSlug = null,
                ChildCount = linha.ContagemFilhosDiretos,
                GroupKey = linha.ChaveNivelAtual
            };
        }

        var p = linha.Produto!;
        return new ProdutoGridRowDto
        {
            IsGroup = false,
            Id = p.Id,
            Nome = p.Nome,
            UnidadeComercializacao = p.UnidadeComercializacao,
            UnidadeMedidaFisica = p.UnidadeMedidaFisica,
            TipoEmbalagem = p.TipoEmbalagem,
            Marca = p.Marca,
            CategoriaNome = p.CategoriaProduto?.Nome,
            CategoriaSlug = p.CategoriaProduto?.Slug,
            ChildCount = 0,
            GroupKey = string.Empty
        };
    }

    public async Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorNcmAsync(string ncm, CancellationToken cancellationToken = default)
    {
        var digitos = SomenteDigitos(ncm);
        if (digitos.Length != 8)
            throw new BusinessException("PRODUTO_CONSULTA_NCM_INVALIDO", "Informe um NCM com 8 dígitos numéricos.");

        var lista = await _uw.ProdutoRepository.BuscarPorNcmAsync(digitos, cancellationToken);
        return lista.Select(MapearListItem).ToList();
    }

    public async Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorOrigemGeograficaAsync(
        string tipoOrigemCodigo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tipoOrigemCodigo))
            throw new BusinessException("PRODUTO_CONSULTA_ORIGEM_OBRIGATORIA", "Tipo de origem geográfica é obrigatório na consulta.");

        var tipo = tipoOrigemCodigo.Trim().ToUpperInvariant();
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoOrigemGeografica,
                tipo,
                cancellationToken))
            throw new BusinessException("PRODUTO_CONSULTA_ORIGEM_INVALIDA", "Tipo de origem geográfica não está cadastrado nos parâmetros.");

        var lista = await _uw.ProdutoRepository.BuscarPorOrigemGeograficaAsync(tipo, cancellationToken);
        return lista.Select(MapearListItem).ToList();
    }

    public async Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorUnidadeMedidaAsync(
        string unidadeMedida,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(unidadeMedida))
            throw new BusinessException("PRODUTO_CONSULTA_UNIDADE_OBRIGATORIA", "Unidade de medida é obrigatória na consulta.");

        var unidade = NormalizarCodigoParametro(
            unidadeMedida,
            "PRODUTO_CONSULTA_UNIDADE_OBRIGATORIA",
            "Unidade de medida é obrigatória na consulta.");
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadeMedida,
                unidade,
                cancellationToken))
            throw new BusinessException("PRODUTO_UNIDADE_MEDIDA_CATALOGO", $"Unidade de medida física '{unidade}' não está cadastrada nos parâmetros.");

        var lista = await _uw.ProdutoRepository.BuscarPorUnidadeMedidaAsync(unidade, cancellationToken);
        return lista.Select(MapearListItem).ToList();
    }

    public async Task<IReadOnlyList<ProdutoListItemDto>> BuscarPorMarcaAsync(string marca, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new BusinessException("PRODUTO_CONSULTA_MARCA_OBRIGATORIA", "Marca é obrigatória na consulta.");

        var fragmento = marca.Trim().ToLowerInvariant();
        var lista = await _uw.ProdutoRepository.BuscarPorMarcaContendoAsync(fragmento, cancellationToken);
        return lista.Select(MapearListItem).ToList();
    }

    private async Task ValidarCatalogoProdutoAsync(
        ProdutoCreateDto dto,
        CancellationToken cancellationToken)
    {
        var com = NormalizarCodigoParametro(
            dto.UnidadeComercializacao,
            "PRODUTO_UNIDADE_COMERCIALIZACAO_OBRIGATORIA",
            "Unidade de comercialização é obrigatória.");
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadeComercializacao,
                com,
                cancellationToken))
            throw new BusinessException("PRODUTO_UNIDADE_COMERCIALIZACAO_CATALOGO",
                $"Unidade de comercialização '{com}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");

        var fis = NormalizarCodigoParametro(
            dto.UnidadeMedidaFisica,
            "PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA",
            "Unidade de medida física é obrigatória.");
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadeMedida,
                fis,
                cancellationToken))
            throw new BusinessException("PRODUTO_UNIDADE_MEDIDA_CATALOGO",
                $"Unidade de medida física '{fis}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");

        var emb = NormalizarCodigoParametro(
            dto.TipoEmbalagem,
            "PRODUTO_TIPO_EMBALAGEM_OBRIGATORIO",
            "Tipo de embalagem é obrigatório.");
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadeEmbalagem,
                emb,
                cancellationToken))
            throw new BusinessException("PRODUTO_TIPO_EMBALAGEM_CATALOGO",
                $"Tipo de embalagem '{emb}' não está cadastrado nos parâmetros.{DicaParametrosCategoriaProduto}");

        var dimEmb = NormalizarCodigoParametro(
            dto.DimensaoEmbalagem.UnidadeDimensao,
            "EMBALAGEM_UNIDADE_DIMENSAO_OBRIGATORIA",
            "Unidade das dimensões da embalagem é obrigatória.");
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadeDimensao,
                dimEmb,
                cancellationToken))
            throw new BusinessException("PRODUTO_EMBALAGEM_UNIDADE_DIMENSAO_CATALOGO",
                $"Unidade de dimensão '{dimEmb}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");

        var pesoEmb = NormalizarCodigoParametro(
            dto.DimensaoEmbalagem.UnidadePeso,
            "EMBALAGEM_UNIDADE_PESO_OBRIGATORIA",
            "Unidade de peso da embalagem é obrigatória.");
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadePeso,
                pesoEmb,
                cancellationToken))
            throw new BusinessException("PRODUTO_EMBALAGEM_UNIDADE_PESO_CATALOGO",
                $"Unidade de peso '{pesoEmb}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");

        if (dto.DimensaoProduto is not null)
        {
            var dimP = NormalizarCodigoParametro(
                dto.DimensaoProduto.UnidadeDimensao,
                "PRODUTO_DIMENSAO_UNIDADE_OBRIGATORIA",
                "Unidade das dimensões do produto é obrigatória.");
            if (!await _parametrosConsulta.ExisteChaveAsync(
                    ProdutoParametrosCatalogo.Categoria,
                    ProdutoParametrosCatalogo.AtributoUnidadeDimensao,
                    dimP,
                    cancellationToken))
                throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_CATALOGO",
                    $"Unidade de dimensão '{dimP}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");

            var pesoP = NormalizarCodigoParametro(
                dto.DimensaoProduto.UnidadePeso,
                "PRODUTO_DIMENSAO_UNIDADE_PESO_OBRIGATORIA",
                "Unidade de peso do produto é obrigatória.");
            if (!await _parametrosConsulta.ExisteChaveAsync(
                    ProdutoParametrosCatalogo.Categoria,
                    ProdutoParametrosCatalogo.AtributoUnidadePeso,
                    pesoP,
                    cancellationToken))
                throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_PESO_CATALOGO",
                    $"Unidade de peso '{pesoP}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");
        }

        if (string.IsNullOrWhiteSpace(dto.OrigemGeografica.Tipo))
            throw new BusinessException("PRODUTO_ORIGEM_GEOGRAFICA_OBRIGATORIA", "Tipo de origem geográfica é obrigatório.");

        var tipoGeo = dto.OrigemGeografica.Tipo.Trim().ToUpperInvariant();
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoOrigemGeografica,
                tipoGeo,
                cancellationToken))
            throw new BusinessException("PRODUTO_ORIGEM_GEOGRAFICA_CATALOGO",
                $"Tipo de origem geográfica '{tipoGeo}' não está cadastrado nos parâmetros.{DicaParametrosCategoriaProduto}");

        if (string.IsNullOrWhiteSpace(dto.DadosFiscais.Origem))
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_OBRIGATORIA", "Origem ICMS é obrigatória.");

        var origem = dto.DadosFiscais.Origem.Trim();
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoOrigemIcms,
                origem,
                cancellationToken))
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_CATALOGO",
                $"Origem ICMS '{origem}' não está cadastrada nos parâmetros.{DicaParametrosCategoriaProduto}");

        if (dto.CategoriaProdutoId is { } cid)
        {
            var cat = await _uw.Categorias.ObterPorIdAsync(cid, cancellationToken);
            if (cat is null)
                throw new BusinessException("PRODUTO_CATEGORIA_NAO_ENCONTRADA",
                    $"Não existe categoria de produto com o identificador {cid}. Verifique o código enviado ou cadastre a categoria antes de vincular.");
            if (!cat.Ativo)
                throw new BusinessException("PRODUTO_CATEGORIA_INATIVA",
                    $"A categoria '{cat.Nome}' (id {cid}) está inativa. Ative a categoria ou escolha outra para vincular o produto.");
        }
    }

    private static string NormalizarCodigoParametro(string codigo, string errorCode, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new BusinessException(errorCode, errorMessage);

        var c = codigo.Trim().ToUpperInvariant();
        if (c.Length == 0 || c.Length > 16)
            throw new BusinessException(errorCode,
                    $"{errorMessage} O código deve ter de 1 a 16 caracteres alfanuméricos, sem espaços.")
                .With("CodigoInformado", codigo);

        foreach (var ch in c.AsSpan())
        {
            if (!char.IsLetterOrDigit(ch))
                throw new BusinessException(errorCode,
                        $"{errorMessage} Use apenas letras e números (A–Z, 0–9), sem espaços ou símbolos.")
                    .With("CodigoInformado", codigo);
        }

        return c;
    }

    private static ProdutoListItemDto MapearListItem(ProdutoEntity p) =>
        new()
        {
            Id = p.Id,
            Nome = p.Nome,
            Marca = p.Marca,
            Modelo = p.Modelo,
            Gtin = p.Gtin,
            UnidadeComercializacao = p.UnidadeComercializacao,
            UnidadeMedidaFisica = p.UnidadeMedidaFisica,
            TipoEmbalagem = p.TipoEmbalagem,
            Ncm = p.DadosFiscais.Ncm,
            OrigemGeograficaTipo = p.OrigemProduto.Tipo,
            OrigemGeograficaPais = p.OrigemProduto.PaisOrigem
        };

    private static string SomenteDigitos(string entrada)
    {
        var sb = new StringBuilder(entrada.Length);
        foreach (var c in entrada.AsSpan())
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static ProdutoResponseDto MapearResposta(ProdutoEntity p)
    {
        ProdutoDimensaoDto? dimP = null;
        if (p.DimensaoProduto is not null)
        {
            dimP = new ProdutoDimensaoDto
            {
                Altura = p.DimensaoProduto.Altura,
                Largura = p.DimensaoProduto.Largura,
                Comprimento = p.DimensaoProduto.Comprimento,
                Peso = p.DimensaoProduto.Peso,
                UnidadeDimensao = p.DimensaoProduto.UnidadeDimensao,
                UnidadePeso = p.DimensaoProduto.UnidadePeso
            };
        }

        var cat = p.CategoriaProduto;
        var caminho = MontarCaminhoCategoria(cat);

        return new ProdutoResponseDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            Marca = p.Marca,
            Modelo = p.Modelo,
            Gtin = p.Gtin,
            CategoriaProdutoId = p.CategoriaProdutoId,
            CategoriaNome = cat?.Nome,
            CategoriaSlug = cat?.Slug,
            CategoriaCaminho = caminho,
            UnidadeComercializacao = p.UnidadeComercializacao,
            UnidadeMedidaFisica = p.UnidadeMedidaFisica,
            TipoEmbalagem = p.TipoEmbalagem,
            OrigemGeograficaTipo = p.OrigemProduto.Tipo,
            OrigemGeograficaPais = p.OrigemProduto.PaisOrigem,
            DadosFiscais = new ProdutoDadosFiscaisDto
            {
                Ncm = p.DadosFiscais.Ncm,
                Cest = p.DadosFiscais.Cest,
                Origem = p.DadosFiscais.Origem
            },
            DimensaoProduto = dimP,
            DimensaoEmbalagem = new ProdutoDimensaoEmbalagemDto
            {
                Altura = p.DimensaoEmbalagem.Altura,
                Largura = p.DimensaoEmbalagem.Largura,
                Comprimento = p.DimensaoEmbalagem.Comprimento,
                Peso = p.DimensaoEmbalagem.Peso,
                UnidadeDimensao = p.DimensaoEmbalagem.UnidadeDimensao,
                UnidadePeso = p.DimensaoEmbalagem.UnidadePeso
            },
            Skus = p.Skus
                .OrderBy(s => s.Id)
                .Select(s => new ProdutoSkuResponseDto
                {
                    Id = s.Id,
                    Codigo = s.Codigo,
                    Ativo = s.Ativo
                })
                .ToList(),
            Atributos = p.Atributos
                .Select(a => new ProdutoAtributoDto { Nome = a.Nome, Valor = a.Valor })
                .ToList()
        };
    }

    private static IReadOnlyList<string> MontarCaminhoCategoria(CategoriaProdutoEntity? no)
    {
        if (no is null)
            return Array.Empty<string>();

        var nomes = new List<string>();
        for (CategoriaProdutoEntity? c = no; c is not null; c = c.CategoriaPai)
            nomes.Insert(0, c.Nome);

        return nomes;
    }
}
