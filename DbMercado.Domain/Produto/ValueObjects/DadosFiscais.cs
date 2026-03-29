using System.Text;
using DbMercado.Domain.Shared.Exceptions;

namespace DbMercado.Domain.Produto.ValueObjects;

/// <summary>
/// Dados fiscais do produto (NCM, CEST opcional e origem da mercadoria para ICMS).
/// A origem ICMS é o código da chave em parâmetros (ex.: 0, 1, 2).
/// </summary>
public sealed class DadosFiscais : IEquatable<DadosFiscais>
{
    private DadosFiscais()
    {
    }

    /// <summary>NCM com exatamente 8 dígitos numéricos.</summary>
    public string Ncm { get; private set; } = string.Empty;

    /// <summary>CEST normalizado (7 dígitos) quando informado.</summary>
    public string? Cest { get; private set; }

    /// <summary>Código de origem da mercadoria para ICMS (chave em parâmetros, ex.: 0, 1, 2).</summary>
    public string Origem { get; private set; } = string.Empty;

    public static DadosFiscais Criar(string ncm, string? cest, string origemIcms)
    {
        var ncmNorm = NormalizarNcm(ncm);
        var cestNorm = NormalizarCestOpcional(cest);
        var origemNorm = NormalizarOrigemIcms(origemIcms);

        return new DadosFiscais
        {
            Ncm = ncmNorm,
            Cest = cestNorm,
            Origem = origemNorm
        };
    }

    /// <summary>
    /// Garante que o estado persistido do value object respeita o formato fiscal (defesa contra dados inconsistentes).
    /// </summary>
    public void GarantirInvariantes()
    {
        if (string.IsNullOrWhiteSpace(Ncm) || Ncm.Length != 8 || !TodosDigitos(Ncm))
            throw new BusinessException("PRODUTO_NCM_INVALIDO", "NCM deve conter exatamente 8 dígitos numéricos.")
                .With("NcmArmazenado", Ncm);

        if (Cest is not null && (Cest.Length != 7 || !TodosDigitos(Cest)))
            throw new BusinessException("PRODUTO_CEST_INVALIDO", "CEST armazenado deve ter 7 dígitos numéricos.")
                .With("CestArmazenado", Cest);

        ValidarFormatoOrigemIcmsArmazenada(Origem);
    }

    private static void ValidarFormatoOrigemIcmsArmazenada(string origem)
    {
        if (string.IsNullOrWhiteSpace(origem) || origem.Length > 8)
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_INVALIDA", "Origem ICMS armazenada é inválida.")
                .With("OrigemArmazenada", origem);

        foreach (var c in origem.AsSpan())
        {
            if (c < '0' || c > '9')
                throw new BusinessException("PRODUTO_ORIGEM_ICMS_INVALIDA", "Origem ICMS armazenada é inválida.")
                    .With("OrigemArmazenada", origem);
        }
    }

    private static bool TodosDigitos(string valor)
    {
        foreach (var c in valor.AsSpan())
        {
            if (c < '0' || c > '9')
                return false;
        }

        return true;
    }

    private static string NormalizarNcm(string? ncm)
    {
        if (string.IsNullOrWhiteSpace(ncm))
            throw new BusinessException("PRODUTO_NCM_OBRIGATORIO", "NCM é obrigatório.");

        var digitos = SomenteDigitos(ncm);
        if (digitos.Length != 8)
            throw new BusinessException("PRODUTO_NCM_INVALIDO", "NCM deve conter exatamente 8 dígitos numéricos.")
                .With("NcmInformado", ncm);

        return digitos;
    }

    private static string? NormalizarCestOpcional(string? cest)
    {
        if (string.IsNullOrWhiteSpace(cest))
            return null;

        var digitos = SomenteDigitos(cest);
        if (digitos.Length != 7)
            throw new BusinessException("PRODUTO_CEST_INVALIDO", "CEST, quando informado, deve conter 7 dígitos numéricos.")
                .With("CestInformado", cest);

        return digitos;
    }

    private static string NormalizarOrigemIcms(string? origemIcms)
    {
        if (string.IsNullOrWhiteSpace(origemIcms))
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_OBRIGATORIA", "Origem ICMS é obrigatória.");

        var o = origemIcms.Trim();
        ValidarFormatoOrigemIcmsArmazenada(o);
        return o;
    }

    private static string SomenteDigitos(string entrada)
    {
        var sb = new StringBuilder(entrada.Length);
        foreach (var c in entrada.AsSpan())
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    public bool Equals(DadosFiscais? other) =>
        other is not null
        && Ncm == other.Ncm
        && string.Equals(Cest, other.Cest, StringComparison.Ordinal)
        && Origem == other.Origem;

    public override bool Equals(object? obj) => obj is DadosFiscais d && Equals(d);

    public override int GetHashCode() => HashCode.Combine(Ncm, Cest ?? string.Empty, Origem);
}
