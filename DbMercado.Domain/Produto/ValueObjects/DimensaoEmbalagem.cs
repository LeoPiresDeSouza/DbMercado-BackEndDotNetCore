using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dimensões e peso da embalagem logística do produto.
/// </summary>
public sealed class DimensaoEmbalagem : IEquatable<DimensaoEmbalagem>
{
    private DimensaoEmbalagem()
    {
    }

    public decimal Altura { get; private set; }

    public decimal Largura { get; private set; }

    public decimal Comprimento { get; private set; }

    public decimal Peso { get; private set; }

    public static DimensaoEmbalagem Criar(decimal altura, decimal largura, decimal comprimento, decimal peso)
    {
        if (altura <= 0)
            throw new BusinessException("EMBALAGEM_ALTURA_INVALIDA", "Altura da embalagem deve ser maior que zero.");
        if (largura <= 0)
            throw new BusinessException("EMBALAGEM_LARGURA_INVALIDA", "Largura da embalagem deve ser maior que zero.");
        if (comprimento <= 0)
            throw new BusinessException("EMBALAGEM_COMPRIMENTO_INVALIDO", "Comprimento da embalagem deve ser maior que zero.");
        if (peso <= 0)
            throw new BusinessException("EMBALAGEM_PESO_INVALIDO", "Peso da embalagem deve ser maior que zero.");

        return new DimensaoEmbalagem
        {
            Altura = altura,
            Largura = largura,
            Comprimento = comprimento,
            Peso = peso
        };
    }

    /// <summary>Revalida dimensões e peso após materialização ou alteração (valores estritamente positivos).</summary>
    public void GarantirInvariantes()
    {
        GarantirPositivo(Altura, "EMBALAGEM_ALTURA_INVALIDA", "Altura da embalagem deve ser maior que zero.");
        GarantirPositivo(Largura, "EMBALAGEM_LARGURA_INVALIDA", "Largura da embalagem deve ser maior que zero.");
        GarantirPositivo(Comprimento, "EMBALAGEM_COMPRIMENTO_INVALIDO", "Comprimento da embalagem deve ser maior que zero.");
        GarantirPositivo(Peso, "EMBALAGEM_PESO_INVALIDO", "Peso da embalagem deve ser maior que zero.");
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
        && Peso == other.Peso;

    public override bool Equals(object? obj) => obj is DimensaoEmbalagem d && Equals(d);

    public override int GetHashCode() => HashCode.Combine(Altura, Largura, Comprimento, Peso);
}
