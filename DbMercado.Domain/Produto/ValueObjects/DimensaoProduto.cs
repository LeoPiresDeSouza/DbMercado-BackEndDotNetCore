using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dimensões físicas do produto (sem embalagem comercial), em unidade de medida definida pela aplicação.
/// </summary>
public sealed class DimensaoProduto : IEquatable<DimensaoProduto>
{
    /// <summary>Construtor para materialização pelo ORM.</summary>
    private DimensaoProduto()
    {
    }

    public decimal Altura { get; private set; }

    public decimal Largura { get; private set; }

    public decimal Comprimento { get; private set; }

    public static DimensaoProduto Criar(decimal altura, decimal largura, decimal comprimento)
    {
        if (altura <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_ALTURA_INVALIDA", "Altura do produto deve ser maior que zero.");
        if (largura <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_LARGURA_INVALIDA", "Largura do produto deve ser maior que zero.");
        if (comprimento <= 0)
            throw new BusinessException("PRODUTO_DIMENSAO_COMPRIMENTO_INVALIDO", "Comprimento do produto deve ser maior que zero.");

        return new DimensaoProduto
        {
            Altura = altura,
            Largura = largura,
            Comprimento = comprimento
        };
    }

    /// <summary>Revalida medidas após materialização ou alteração (valores estritamente positivos).</summary>
    public void GarantirInvariantes()
    {
        GarantirPositivo(Altura, "PRODUTO_DIMENSAO_ALTURA_INVALIDA", "Altura do produto deve ser maior que zero.");
        GarantirPositivo(Largura, "PRODUTO_DIMENSAO_LARGURA_INVALIDA", "Largura do produto deve ser maior que zero.");
        GarantirPositivo(Comprimento, "PRODUTO_DIMENSAO_COMPRIMENTO_INVALIDO", "Comprimento do produto deve ser maior que zero.");
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
        && Comprimento == other.Comprimento;

    public override bool Equals(object? obj) => obj is DimensaoProduto d && Equals(d);

    public override int GetHashCode() => HashCode.Combine(Altura, Largura, Comprimento);
}
