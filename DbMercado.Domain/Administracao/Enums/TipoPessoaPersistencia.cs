namespace DbMercado.Domain.Administracao.Enums;

/// <summary>
/// Conversão entre <see cref="TipoPessoa"/> e o valor textual da coluna <c>TipoPessoa</c> (compatível com dados legados).
/// </summary>
public static class TipoPessoaPersistencia
{
    public static string ParaArmazenamento(TipoPessoa tipo) =>
        tipo switch
        {
            TipoPessoa.Fisica => "Fisica",
            TipoPessoa.Juridica => "Juridica",
            TipoPessoa.Indefinido => throw new InvalidOperationException("Tipo de pessoa não definido."),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
        };

    public static TipoPessoa DeArmazenamento(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Tipo de pessoa não informado.", nameof(valor));

        var n = valor.Trim();
        return n.ToLowerInvariant() switch
        {
            "fisica" or "física" => TipoPessoa.Fisica,
            "juridica" or "jurídica" => TipoPessoa.Juridica,
            _ when Enum.TryParse<TipoPessoa>(n, ignoreCase: true, out var parsed) => parsed,
            _ => throw new ArgumentException($"Tipo de pessoa desconhecido: {valor}", nameof(valor))
        };
    }
}
