using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Par nome/valor para atributos flexíveis do catálogo (ex.: cor, voltagem).
/// </summary>
public sealed class AtributoProduto : IEquatable<AtributoProduto>
{
    private AtributoProduto()
    {
    }

    public string Nome { get; private set; } = string.Empty;

    public string Valor { get; private set; } = string.Empty;

    public static AtributoProduto Criar(string nome, string valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessException("PRODUTO_ATRIBUTO_NOME_OBRIGATORIO", "Nome do atributo do produto é obrigatório.");

        var v = valor?.Trim() ?? string.Empty;
        return new AtributoProduto
        {
            Nome = nome.Trim(),
            Valor = v
        };
    }

    public bool Equals(AtributoProduto? other) =>
        other is not null
        && string.Equals(Nome, other.Nome, StringComparison.Ordinal)
        && string.Equals(Valor, other.Valor, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is AtributoProduto a && Equals(a);

    public override int GetHashCode() => HashCode.Combine(Nome, Valor);
}
