using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dimensões físicas e peso do produto (sem embalagem comercial).
/// </summary>
public sealed class DimensaoProduto : IEquatable<DimensaoProduto>
{
    private DimensaoProduto() { }

    public decimal Altura { get; private set; }
    public decimal Largura { get; private set; }
    public decimal Comprimento { get; private set; }
    public decimal Peso { get; private set; }

    /// <summary>Unidade das medidas lineares (código do parâmetro: CM, M, MM).</summary>
    public string UnidadeDimensao { get; private set; } = string.Empty;

    /// <summary>Unidade de massa do produto (código do parâmetro: KG, G, T).</summary>
    public string UnidadePeso { get; private set; } = string.Empty;

    public static DimensaoProduto Criar(
        decimal altura,
        decimal largura,
        decimal comprimento,
        decimal peso,
        string unidadeDimensao,
        string unidadePeso)
    {
        if (altura <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_ALTURA_INVALIDA", "Altura do produto deve ser maior que zero.");
        if (largura <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_LARGURA_INVALIDA", "Largura do produto deve ser maior que zero.");
        if (comprimento <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_COMPRIMENTO_INVALIDO", "Comprimento do produto deve ser maior que zero.");
        if (peso <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_PESO_INVALIDO", "Peso do produto deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(unidadeDimensao))
            throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_OBRIGATORIA", "Unidade das dimensões do produto é obrigatória.");
        if (string.IsNullOrWhiteSpace(unidadePeso))
            throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_PESO_OBRIGATORIA", "Unidade de peso do produto é obrigatória.");

        return new DimensaoProduto
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
        GarantirPositivo(Altura, "PRODUTO_DIMENSAO_ALTURA_INVALIDA", "Altura do produto deve ser maior que zero.");
        GarantirPositivo(Largura, "PRODUTO_DIMENSAO_LARGURA_INVALIDA", "Largura do produto deve ser maior que zero.");
        GarantirPositivo(Comprimento, "PRODUTO_DIMENSAO_COMPRIMENTO_INVALIDO", "Comprimento do produto deve ser maior que zero.");
        GarantirPositivo(Peso, "PRODUTO_DIMENSAO_PESO_INVALIDO", "Peso do produto deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(UnidadeDimensao))
            throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_OBRIGATORIA", "Unidade das dimensões do produto é obrigatória.");
        if (string.IsNullOrWhiteSpace(UnidadePeso))
            throw new BusinessException("PRODUTO_DIMENSAO_UNIDADE_PESO_OBRIGATORIA", "Unidade de peso do produto é obrigatória.");
    }

    private static void GarantirPositivo(decimal valor, string codigo, string mensagem)
    {
        if (valor <= 0)
            throw new BusinessException(codigo, mensagem).With("ValorInformado", valor);
    }

    public bool Equals(DimensaoProduto? other) =>
        other is not null
        && Altura == other.Altura
        && Largura == other.Largura
        && Comprimento == other.Comprimento
        && Peso == other.Peso
        && string.Equals(UnidadeDimensao, other.UnidadeDimensao, StringComparison.Ordinal)
        && string.Equals(UnidadePeso, other.UnidadePeso, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is DimensaoProduto d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Altura, Largura, Comprimento, Peso, UnidadeDimensao, UnidadePeso);
}
