using DbMercado.Domain.Produto.Constants;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Origem geográfica do produto e país de origem quando aplicável.
/// O tipo é o código da chave em parâmetros (ex.: 1, 2).
/// </summary>
public sealed class OrigemProduto : IEquatable<OrigemProduto>
{
    private OrigemProduto()
    {
    }

    public string Tipo { get; private set; } = string.Empty;

    /// <summary>País de origem; obrigatório quando <see cref="Tipo"/> é importado.</summary>
    public string? PaisOrigem { get; private set; }

    public static OrigemProduto Criar(string tipoOrigemCodigo, string? paisOrigem)
    {
        if (string.IsNullOrWhiteSpace(tipoOrigemCodigo))
            throw new BusinessException("PRODUTO_ORIGEM_GEOGRAFICA_OBRIGATORIA", "Tipo de origem geográfica é obrigatório.");

        var tipo = tipoOrigemCodigo.Trim().ToUpperInvariant();
        var pais = string.IsNullOrWhiteSpace(paisOrigem) ? null : paisOrigem.Trim();

        if (tipo == OrigemGeograficaProdutoCodigos.Importado && string.IsNullOrEmpty(pais))
            throw new BusinessException("PRODUTO_PAIS_ORIGEM_OBRIGATORIO",
                    "Para origem geográfica importada, o país de origem é obrigatório. Informe o país conforme a operação (ex.: código ISO ou nome).")
                .With("TipoOrigemCodigo", tipo);

        return new OrigemProduto
        {
            Tipo = tipo,
            PaisOrigem = pais
        };
    }

    public bool Equals(OrigemProduto? other) =>
        other is not null
        && string.Equals(Tipo, other.Tipo, StringComparison.Ordinal)
        && string.Equals(PaisOrigem, other.PaisOrigem, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is OrigemProduto o && Equals(o);

    public override int GetHashCode() => HashCode.Combine(Tipo, PaisOrigem ?? string.Empty);
}
