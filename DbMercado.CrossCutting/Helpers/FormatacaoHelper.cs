using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DbMercado.CrossCutting.Helpers;


public static class FormatcaoHelper
{

    /// <summary>
    /// Formatar uma string CNPJ
    /// </summary>
    /// <param name="CNPJ">string CNPJ sem formatacao</param>
    /// <returns>string CNPJ formatada</returns>
    /// <example>Recebe '99999999999999' Devolve '99.999.999/9999-99'</example>

    public static string FormatCNPJ(string CNPJ)
    {
        return Convert.ToUInt64(CNPJ).ToString(@"00\.000\.000\/0000\-00");
    }

    /// <summary>
    /// Formatar uma string CPF
    /// </summary>
    /// <param name="CPF">string CPF sem formatacao</param>
    /// <returns>string CPF formatada</returns>
    /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>

    public static string FormatCPF(string CPF)
    {
        return Convert.ToUInt64(CPF).ToString(@"000\.000\.000\-00");
    }


    /// <summary>
    /// Retira a Formatacao de uma string CNPJ/CPF
    /// </summary>
    /// <param name="Codigo">string Codigo Formatada</param>
    /// <returns>string sem formatacao</returns>
    /// <example>Recebe '99.999.999/9999-99' Devolve '99999999999999'</example>

    public static string SemFormatacao(string Codigo)
    {
        return Codigo.Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty);
    }



    /// <summary>
    /// Retira os acentos, caracteres especiais e espaços de um texto em função dos parâmetros fornecidos.
    /// Também pode retornar todo o rexto em maiúscula ou minúscula.
    /// Caso tanto a opção de maiúscula e minúscula seja solicitada, o texto sempre retornará em minúscula.
    /// </summary>
    /// <param name="texto">Texto a ser normalizado.</param>
    /// <param name="removeSpaces">Informa se os espaços no internos devem ser removidos.</param>
    /// <param name="removeSpecialCharacteres">Informa se os caracteres especiais devem ser removidos do texto.</param>
    /// <param name="toUpper">Informa se o texto final deve seguir em maiúscula.</param>
    /// <param name="toLower">Informa se o texto final deve retornar em minúscula.</param>
    /// <returns>Texto normalizado.</returns>
    public static string NormalizeText(string texto, bool removeSpaces = false, bool removeSpecialCharacteres = false, bool toUpper = false, bool toLower = false)
    {
        string result = RemoveAcentos(texto);
        if (removeSpaces) result = RemoveSpaces(result);
        if (removeSpecialCharacteres) result = RemoveSpecialCharacters(result);
        if (toUpper) result = result.ToUpper();
        if (toLower) result = result.ToLower();

        return result;
    }



    /// <summary>
    /// Substitui os acentos das palavras de um texto pelo seu caracter básico.
    /// </summary>
    /// <param name="texto">Texto a ser revisdado.</param>
    /// <returns>Texto revisado.</returns>
    public static string RemoveAcentos(string texto)
    {
        string normalizedWord = texto.Normalize(NormalizationForm.FormD);

        StringBuilder sb = new StringBuilder();

        foreach (char c in normalizedWord)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString();
    }



    /// <summary>
    /// Remove espaços entre as palavrs de um texto.
    /// </summary>
    /// <param name="texto">Texto a ser revisdado.</param>
    /// <returns>Texto revisado.</returns>
    public static string RemoveSpaces(string texto)
    {
        string withoutSpaces = texto.Replace(" ", "");
        return withoutSpaces;
    }



    /// <summary>
    /// Remove os caracteres especiais de um texto.
    /// </summary>
    /// <param name="texto">Texto a ser revisdado.</param>
    /// <returns>Texto revisado.</returns>
    public static string RemoveSpecialCharacters(string text)
    {
        string pattern = @"[^a-zA-Z0-9\s]";
        string withoutSpecialCharacters = Regex.Replace(text, pattern, "");
        return withoutSpecialCharacters;
    }

}
