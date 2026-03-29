using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Código de unidade de medida comercial (normalizado em maiúsculas). Os valores permitidos vêm de <c>ParametroEntity</c> (categoria produto, atributo unidadeMedida).
/// </summary>
public sealed class CodigoUnidadeMedidaProduto : IEquatable<CodigoUnidadeMedidaProduto>
{
    private const int TamanhoMaximo = 16;

    private CodigoUnidadeMedidaProduto()
    {
    }

    public string Codigo { get; private set; } = string.Empty;

    public static CodigoUnidadeMedidaProduto Criar(string entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            throw new BusinessException("PRODUTO_UNIDADE_MEDIDA_OBRIGATORIA", "Unidade de medida é obrigatória.");

        var c = entrada.Trim().ToUpperInvariant();
        if (c.Length == 0 || c.Length > TamanhoMaximo)
            throw new BusinessException("PRODUTO_UNIDADE_MEDIDA_INVALIDA", "Unidade de medida deve ter entre 1 e 16 caracteres.")
                .With("CodigoInformado", entrada);

        foreach (var ch in c.AsSpan())
        {
            if (!char.IsLetterOrDigit(ch))
                throw new BusinessException("PRODUTO_UNIDADE_MEDIDA_INVALIDA", "Unidade de medida deve conter apenas letras e números.")
                    .With("CodigoInformado", entrada);
        }

        return new CodigoUnidadeMedidaProduto { Codigo = c };
    }

    public bool Equals(CodigoUnidadeMedidaProduto? other) =>
        other is not null && string.Equals(Codigo, other.Codigo, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is CodigoUnidadeMedidaProduto u && Equals(u);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Codigo);
}
