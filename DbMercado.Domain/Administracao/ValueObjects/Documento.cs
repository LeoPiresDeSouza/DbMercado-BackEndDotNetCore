using System.Text;
using DbMercado.Domain.Administracao.Enums;

namespace DbMercado.Domain.Administracao.ValueObjects;

/// <summary>
/// Documento fiscal brasileiro (CPF ou CNPJ), imutável, com validação de dígitos verificadores.
/// </summary>
public sealed class Documento : IEquatable<Documento>
{
    private Documento(string valorNormalizado, TipoDocumentoFiscal tipo)
    {
        ValorNormalizado = valorNormalizado;
        Tipo = tipo;
    }

    public string ValorNormalizado { get; }

    public TipoDocumentoFiscal Tipo { get; }

    public bool EhCpf => Tipo == TipoDocumentoFiscal.Cpf;

    public bool EhCnpj => Tipo == TipoDocumentoFiscal.Cnpj;

    /// <summary>
    /// Cria CPF a partir de entrada com ou sem máscara (apenas dígitos são considerados).
    /// </summary>
    public static Documento CriarCpf(string entrada)
    {
        var digitos = SomenteDigitos(entrada);
        if (digitos.Length != 11)
            throw new ArgumentException("CPF deve conter 11 dígitos.", nameof(entrada));

        if (TodosDigitosIguais(digitos))
            throw new ArgumentException("CPF inválido.", nameof(entrada));

        if (!ValidarDigitosCpf(digitos))
            throw new ArgumentException("CPF inválido (dígitos verificadores).", nameof(entrada));

        return new Documento(digitos, TipoDocumentoFiscal.Cpf);
    }

    /// <summary>
    /// Cria CNPJ a partir de entrada com ou sem máscara.
    /// </summary>
    public static Documento CriarCnpj(string entrada)
    {
        var digitos = SomenteDigitos(entrada);
        if (digitos.Length != 14)
            throw new ArgumentException("CNPJ deve conter 14 dígitos.", nameof(entrada));

        if (TodosDigitosIguais(digitos))
            throw new ArgumentException("CNPJ inválido.", nameof(entrada));

        if (!ValidarDigitosCnpj(digitos))
            throw new ArgumentException("CNPJ inválido (dígitos verificadores).", nameof(entrada));

        return new Documento(digitos, TipoDocumentoFiscal.Cnpj);
    }

    public string ObterFormatado()
    {
        return Tipo switch
        {
            TipoDocumentoFiscal.Cpf when ValorNormalizado.Length == 11 =>
                $"{ValorNormalizado[..3]}.{ValorNormalizado.Substring(3, 3)}.{ValorNormalizado.Substring(6, 3)}-{ValorNormalizado.Substring(9, 2)}",
            TipoDocumentoFiscal.Cnpj when ValorNormalizado.Length == 14 =>
                $"{ValorNormalizado[..2]}.{ValorNormalizado.Substring(2, 3)}.{ValorNormalizado.Substring(5, 3)}/{ValorNormalizado.Substring(8, 4)}-{ValorNormalizado.Substring(12, 2)}",
            _ => ValorNormalizado
        };
    }

    public bool Equals(Documento? other) =>
        other is not null && ValorNormalizado == other.ValorNormalizado && Tipo == other.Tipo;

    public override bool Equals(object? obj) => obj is Documento d && Equals(d);

    public override int GetHashCode() => HashCode.Combine(ValorNormalizado, Tipo);

    public override string ToString() => ObterFormatado();

    private static string SomenteDigitos(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            return string.Empty;

        var sb = new StringBuilder(entrada.Length);
        foreach (var c in entrada.AsSpan())
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static bool TodosDigitosIguais(string digitos)
    {
        if (digitos.Length == 0)
            return true;
        var primeiro = digitos[0];
        for (var i = 1; i < digitos.Length; i++)
        {
            if (digitos[i] != primeiro)
                return false;
        }

        return true;
    }

    private static bool ValidarDigitosCpf(string digitos)
    {
        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += (digitos[i] - '0') * (10 - i);
        var resto = soma % 11;
        var dv1 = resto < 2 ? 0 : 11 - resto;
        if (digitos[9] - '0' != dv1)
            return false;

        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += (digitos[i] - '0') * (11 - i);
        resto = soma % 11;
        var dv2 = resto < 2 ? 0 : 11 - resto;
        return digitos[10] - '0' == dv2;
    }

    private static bool ValidarDigitosCnpj(string digitos)
    {
        ReadOnlySpan<int> pesos1 = stackalloc int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        ReadOnlySpan<int> pesos2 = stackalloc int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var soma = 0;
        for (var i = 0; i < 12; i++)
            soma += (digitos[i] - '0') * pesos1[i];
        var resto = soma % 11;
        var dv1 = resto < 2 ? 0 : 11 - resto;
        if (digitos[12] - '0' != dv1)
            return false;

        soma = 0;
        for (var i = 0; i < 13; i++)
            soma += (digitos[i] - '0') * pesos2[i];
        resto = soma % 11;
        var dv2 = resto < 2 ? 0 : 11 - resto;
        return digitos[13] - '0' == dv2;
    }
}
