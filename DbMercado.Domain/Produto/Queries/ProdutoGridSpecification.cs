namespace DbMercado.Domain.Produto.Queries;

/// <summary>
/// Consulta de catálogo para grid (paginação, ordenação e filtros interpretados no servidor).
/// </summary>
public sealed class ProdutoGridSpecification
{
    public const int TamanhoMaximoPagina = 200;

    public required int Skip { get; init; }

    public required int Take { get; init; }

    public IReadOnlyList<ProdutoGridOrdenacao> Ordenacao { get; init; } = Array.Empty<ProdutoGridOrdenacao>();

    public ProdutoGridFiltro Filtro { get; init; } = new();

    /// <summary>Campos permitidos para agrupamento, na ordem do painel (SSRM). Vazio = lista plana.</summary>
    public IReadOnlyList<string> CamposAgrupamento { get; init; } = Array.Empty<string>();

    /// <summary>Chaves do caminho de grupo expandido (uma entrada por nível).</summary>
    public IReadOnlyList<string> ChavesGrupo { get; init; } = Array.Empty<string>();

    /// <summary>Quando verdadeiro, linhas de grupo preenchem <c>Id</c> com a contagem (painel Valores / agg count).</summary>
    public bool AgregarContagemId { get; init; }

    /// <summary>Filtro opcional do painel (categoria + descendentes).</summary>
    public long? CategoriaIdFiltro { get; init; }

    /// <summary>Filtro opcional do painel: rótulos <c>NACIONAL</c> / <c>IMPORTADO</c> ou códigos <c>1</c> / <c>2</c>.</summary>
    public string? OrigemFiltro { get; init; }
}

public sealed class ProdutoGridOrdenacao
{
    /// <summary>Campo permitido: id, nome, marca, unidadeMedida.</summary>
    public required string Campo { get; init; }

    public required bool Crescente { get; init; }
}

public sealed class ProdutoGridFiltro
{
    public IReadOnlyList<FiltroNumeroColuna> Id { get; init; } = Array.Empty<FiltroNumeroColuna>();

    public IReadOnlyList<FiltroTextoColuna> Nome { get; init; } = Array.Empty<FiltroTextoColuna>();

    public IReadOnlyList<FiltroTextoColuna> Marca { get; init; } = Array.Empty<FiltroTextoColuna>();

    public IReadOnlyList<FiltroTextoColuna> UnidadeMedida { get; init; } = Array.Empty<FiltroTextoColuna>();
}

public sealed record FiltroTextoColuna(TextoFiltroOperador Operador, string? Valor);

public enum TextoFiltroOperador
{
    Contem,
    NaoContem,
    Igual,
    Diferente,
    ComecaCom,
    TerminaCom,
    EmBranco,
    NaoEmBranco
}

public sealed record FiltroNumeroColuna(NumeroFiltroOperador Operador, long? Valor, long? ValorAte);

public enum NumeroFiltroOperador
{
    Igual,
    Diferente,
    Menor,
    MenorOuIgual,
    Maior,
    MaiorOuIgual,
    Entre,
    EmBranco,
    NaoEmBranco
}
