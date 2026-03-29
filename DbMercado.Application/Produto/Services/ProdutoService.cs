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

namespace DbMercado.Application.Produto.Services;

public class ProdutoService : IProdutoService
{
    private readonly IUwProduto _uw;
    private readonly IParametroChaveConsultaRepository _parametrosConsulta;

    public ProdutoService(IUwProduto uw, IParametroChaveConsultaRepository parametrosConsulta)
    {
        _uw = uw;
        _parametrosConsulta = parametrosConsulta;
    }

    public async Task<long> CriarProdutoAsync(string usuarioAutenticado, ProdutoCreateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.OrigemGeografica);
        ArgumentNullException.ThrowIfNull(dto.DadosFiscais);
        ArgumentNullException.ThrowIfNull(dto.DimensaoEmbalagem);
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioAutenticado);

        await ValidarCatalogoProdutoAsync(
            dto.UnidadeMedida,
            dto.OrigemGeografica.Tipo,
            dto.DadosFiscais.Origem,
            cancellationToken);

        var origem = OrigemProduto.Criar(dto.OrigemGeografica.Tipo, dto.OrigemGeografica.PaisOrigem);
        var dadosFiscais = DadosFiscais.Criar(dto.DadosFiscais.Ncm, dto.DadosFiscais.Cest, dto.DadosFiscais.Origem);
        var embalagem = DimensaoEmbalagem.Criar(
            dto.DimensaoEmbalagem.Altura,
            dto.DimensaoEmbalagem.Largura,
            dto.DimensaoEmbalagem.Comprimento,
            dto.DimensaoEmbalagem.Peso);
        DimensaoProduto? dimProduto = null;
        if (dto.DimensaoProduto is not null)
        {
            dimProduto = DimensaoProduto.Criar(
                dto.DimensaoProduto.Altura,
                dto.DimensaoProduto.Largura,
                dto.DimensaoProduto.Comprimento);
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
            dto.UnidadeMedida,
            dimProduto,
            embalagem,
            origem,
            dadosFiscais,
            atributos,
            skus,
            usuarioAutenticado);

        await _uw.ProdutoRepository.AddAsync(usuarioAutenticado, entidade);
        await _uw.SaveChangesAsync(cancellationToken);
        return entidade.Id;
    }

    public async Task AtualizarProdutoAsync(string usuarioAutenticado, long id, ProdutoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(dto.OrigemGeografica);
        ArgumentNullException.ThrowIfNull(dto.DadosFiscais);
        ArgumentNullException.ThrowIfNull(dto.DimensaoEmbalagem);
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioAutenticado);

        await ValidarCatalogoProdutoAsync(
            dto.UnidadeMedida,
            dto.OrigemGeografica.Tipo,
            dto.DadosFiscais.Origem,
            cancellationToken);

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
            dto.UnidadeMedida,
            origem,
            usuarioAutenticado);

        DimensaoProduto? dimProduto = null;
        if (dto.DimensaoProduto is not null)
        {
            dimProduto = DimensaoProduto.Criar(
                dto.DimensaoProduto.Altura,
                dto.DimensaoProduto.Largura,
                dto.DimensaoProduto.Comprimento);
        }

        var embalagem = DimensaoEmbalagem.Criar(
            dto.DimensaoEmbalagem.Altura,
            dto.DimensaoEmbalagem.Largura,
            dto.DimensaoEmbalagem.Comprimento,
            dto.DimensaoEmbalagem.Peso);
        entidade.AtualizarDimensoes(dimProduto, embalagem, usuarioAutenticado);

        var dadosFiscais = DadosFiscais.Criar(dto.DadosFiscais.Ncm, dto.DadosFiscais.Cest, dto.DadosFiscais.Origem);
        entidade.AtualizarDadosFiscais(dadosFiscais, usuarioAutenticado);

        var atributosLista = dto.Atributos?.Select(a => AtributoProduto.Criar(a.Nome, a.Valor)).ToList() ?? new List<AtributoProduto>();
        entidade.SubstituirAtributos(atributosLista, usuarioAutenticado);

        var skus = dto.Skus.Select(s => (s.Codigo, s.Ativo)).ToList();
        entidade.SincronizarSkus(skus, usuarioAutenticado);

        await _uw.SaveChangesAsync(cancellationToken);
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
                Comprimento = entidade.DimensaoProduto.Comprimento
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
                Peso = entidade.DimensaoEmbalagem.Peso
            },
            UnidadeMedida = entidade.UnidadeMedida
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
                UnidadeMedida = p.UnidadeMedida,
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
            return new ProdutoGridRowDto
            {
                IsGroup = true,
                Id = agregarContagemId ? linha.ContagemFilhosDiretos : null,
                Nome = linha.ValoresAgrupamento.GetValueOrDefault("nome") ?? string.Empty,
                UnidadeMedida = linha.ValoresAgrupamento.GetValueOrDefault("unidadeMedida") ?? string.Empty,
                Marca = marca,
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
            UnidadeMedida = p.UnidadeMedida,
            Marca = p.Marca,
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

        var lista = await _uw.ProdutoRepository.BuscarPorUnidadeMedidaAsync(unidadeMedida.Trim(), cancellationToken);
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
        string unidadeMedida,
        string tipoOrigemGeografica,
        string origemIcms,
        CancellationToken cancellationToken)
    {
        var unidade = CodigoUnidadeMedidaProduto.Criar(unidadeMedida).Codigo;
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoUnidadeMedida,
                unidade,
                cancellationToken))
            throw new BusinessException("PRODUTO_UNIDADE_MEDIDA_CATALOGO", $"Unidade de medida '{unidade}' não está cadastrada nos parâmetros.");

        if (string.IsNullOrWhiteSpace(tipoOrigemGeografica))
            throw new BusinessException("PRODUTO_ORIGEM_GEOGRAFICA_OBRIGATORIA", "Tipo de origem geográfica é obrigatório.");

        var tipoGeo = tipoOrigemGeografica.Trim().ToUpperInvariant();
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoOrigemGeografica,
                tipoGeo,
                cancellationToken))
            throw new BusinessException("PRODUTO_ORIGEM_GEOGRAFICA_CATALOGO", $"Tipo de origem geográfica '{tipoGeo}' não está cadastrado nos parâmetros.");

        if (string.IsNullOrWhiteSpace(origemIcms))
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_OBRIGATORIA", "Origem ICMS é obrigatória.");

        var origem = origemIcms.Trim();
        if (!await _parametrosConsulta.ExisteChaveAsync(
                ProdutoParametrosCatalogo.Categoria,
                ProdutoParametrosCatalogo.AtributoOrigemIcms,
                origem,
                cancellationToken))
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_CATALOGO", $"Origem ICMS '{origem}' não está cadastrada nos parâmetros.");
    }

    private static ProdutoListItemDto MapearListItem(ProdutoEntity p) =>
        new()
        {
            Id = p.Id,
            Nome = p.Nome,
            Marca = p.Marca,
            Modelo = p.Modelo,
            Gtin = p.Gtin,
            UnidadeMedida = p.UnidadeMedida,
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
                Comprimento = p.DimensaoProduto.Comprimento
            };
        }

        return new ProdutoResponseDto
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            Marca = p.Marca,
            Modelo = p.Modelo,
            Gtin = p.Gtin,
            UnidadeMedida = p.UnidadeMedida,
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
                Peso = p.DimensaoEmbalagem.Peso
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
}
