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
            throw new BusinessException("PRODUTO_NCM_INVALIDO",
                    "NCM inválido nos dados persistidos: são necessários exatamente 8 dígitos (0–9). Corrija o cadastro ou reclassifique o produto.")
                .With("NcmArmazenado", Ncm);

        if (Cest is not null && (Cest.Length != 7 || !TodosDigitos(Cest)))
            throw new BusinessException("PRODUTO_CEST_INVALIDO",
                    "CEST inválido nos dados persistidos: quando informado, deve ter 7 dígitos (0–9).")
                .With("CestArmazenado", Cest);

        ValidarFormatoOrigemIcmsArmazenada(Origem);
    }

    private static void ValidarFormatoOrigemIcmsArmazenada(string origem)
    {
        if (string.IsNullOrWhiteSpace(origem))
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_INVALIDA",
                    "Origem ICMS não pode ficar em branco. Informe o código numérico existente nos parâmetros do produto (ex.: 0, 1, 2).")
                .With("OrigemInformada", origem ?? string.Empty);

        if (origem.Length > 8)
            throw new BusinessException("PRODUTO_ORIGEM_ICMS_INVALIDA",
                    $"Origem ICMS aceita no máximo 8 dígitos. Foram informados {origem.Length} caractere(s).")
                .With("OrigemInformada", origem)
                .With("TamanhoInformado", origem.Length);

        foreach (var c in origem.AsSpan())
        {
            if (c < '0' || c > '9')
                throw new BusinessException("PRODUTO_ORIGEM_ICMS_INVALIDA",
                        "Origem ICMS deve conter apenas dígitos (0–9), sem letras ou símbolos, alinhada à tabela de parâmetros.")
                    .With("OrigemInformada", origem);
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

        var digitos = SomenteDigitosAscii(ncm);
        if (digitos.Length != 8)
            throw new BusinessException("PRODUTO_NCM_INVALIDO",
                    $"NCM deve conter exatamente 8 dígitos (0-9). Após remover formatação, foram encontrados {digitos.Length} dígito(s).")
                .With("NcmInformado", ncm)
                .With("DigitosContados", digitos.Length);

        return digitos;
    }

    private static string? NormalizarCestOpcional(string? cest)
    {
        if (string.IsNullOrWhiteSpace(cest))
            return null;

        var digitos = SomenteDigitosAscii(cest);
        if (digitos.Length != 7)
            throw new BusinessException("PRODUTO_CEST_INVALIDO",
                    $"CEST, quando informado, deve conter 7 dígitos (0-9). Após remover formatação, foram encontrados {digitos.Length} dígito(s).")
                .With("CestInformado", cest)
                .With("DigitosContados", digitos.Length);

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

    /// <summary>Apenas caracteres '0'–'9' ASCII (evita dígitos Unicode que quebram validação posterior).</summary>
    private static string SomenteDigitosAscii(string entrada)
    {
        var sb = new StringBuilder(entrada.Length);
        foreach (var c in entrada.AsSpan())
        {
            if (c is >= '0' and <= '9')
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
