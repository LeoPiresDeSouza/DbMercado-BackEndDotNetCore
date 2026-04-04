using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dimensões e peso da embalagem logística do produto.
/// </summary>
public sealed class DimensaoEmbalagem : IEquatable<DimensaoEmbalagem>
{
    private DimensaoEmbalagem() { }

    public decimal Altura { get; private set; }
    public decimal Largura { get; private set; }
    public decimal Comprimento { get; private set; }
    public decimal Peso { get; private set; }

    /// <summary>Unidade das medidas lineares (código do parâmetro: CM, M, MM).</summary>
    public string UnidadeDimensao { get; private set; } = string.Empty;

    /// <summary>Unidade de massa (código do parâmetro: KG, G, T).</summary>
    public string UnidadePeso { get; private set; } = string.Empty;

    public static DimensaoEmbalagem Criar(
        decimal altura,
        decimal largura,
        decimal comprimento,
        decimal peso,
        string unidadeDimensao,
        string unidadePeso)
    {
        if (altura <= 0)
            throw new BusinessException("EMBALAGEM_ALTURA_INVALIDA",
                "Altura da embalagem deve ser maior que zero. Informe um valor decimal positivo (ex.: 10 ou 0,05).");
        if (largura <= 0)
            throw new BusinessException("EMBALAGEM_LARGURA_INVALIDA",
                "Largura da embalagem deve ser maior que zero. Informe um valor decimal positivo (ex.: 10 ou 0,05).");
        if (comprimento <= 0)
            throw new BusinessException("EMBALAGEM_COMPRIMENTO_INVALIDO",
                "Comprimento da embalagem deve ser maior que zero. Informe um valor decimal positivo (ex.: 10 ou 0,05).");
        if (peso <= 0)
            throw new BusinessException("EMBALAGEM_PESO_INVALIDO",
                "Peso da embalagem deve ser maior que zero. Informe um valor decimal positivo (ex.: 0,5 ou 1,2).");
        if (string.IsNullOrWhiteSpace(unidadeDimensao))
            throw new BusinessException("EMBALAGEM_UNIDADE_DIMENSAO_OBRIGATORIA", "Unidade das dimensões da embalagem é obrigatória.");
        if (string.IsNullOrWhiteSpace(unidadePeso))
            throw new BusinessException("EMBALAGEM_UNIDADE_PESO_OBRIGATORIA", "Unidade de peso da embalagem é obrigatória.");

        return new DimensaoEmbalagem
        {
            Altura = altura,
            Largura = largura,
            Comprimento = comprimento,
            Peso = peso,
            UnidadeDimensao = unidadeDimensao.Trim().ToUpperInvariant(),
            UnidadePeso = unidadePeso.Trim().ToUpperInvariant()
        };
    }

    public void GarantirInvariantes()
    {
        GarantirPositivo(Altura, "EMBALAGEM_ALTURA_INVALIDA",
            "Altura da embalagem deve ser maior que zero. Verifique os dados gravados.");
        GarantirPositivo(Largura, "EMBALAGEM_LARGURA_INVALIDA",
            "Largura da embalagem deve ser maior que zero. Verifique os dados gravados.");
        GarantirPositivo(Comprimento, "EMBALAGEM_COMPRIMENTO_INVALIDO",
            "Comprimento da embalagem deve ser maior que zero. Verifique os dados gravados.");
        GarantirPositivo(Peso, "EMBALAGEM_PESO_INVALIDO",
            "Peso da embalagem deve ser maior que zero. Verifique os dados gravados.");

        if (string.IsNullOrWhiteSpace(UnidadeDimensao))
            throw new BusinessException("EMBALAGEM_UNIDADE_DIMENSAO_OBRIGATORIA", "Unidade das dimensões da embalagem é obrigatória.");
        if (string.IsNullOrWhiteSpace(UnidadePeso))
            throw new BusinessException("EMBALAGEM_UNIDADE_PESO_OBRIGATORIA", "Unidade de peso da embalagem é obrigatória.");
    }

    private static void GarantirPositivo(decimal valor, string codigo, string mensagem)
    {
        if (valor <= 0)
            throw new BusinessException(codigo, mensagem).With("ValorInformado", valor);
    }

    public bool Equals(DimensaoEmbalagem? other) =>
        other is not null
        && Altura == other.Altura
        && Largura == other.Largura
        && Comprimento == other.Comprimento
        && Peso == other.Peso
        && string.Equals(UnidadeDimensao, other.UnidadeDimensao, StringComparison.Ordinal)
        && string.Equals(UnidadePeso, other.UnidadePeso, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is DimensaoEmbalagem d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Altura, Largura, Comprimento, Peso, UnidadeDimensao, UnidadePeso);
}
