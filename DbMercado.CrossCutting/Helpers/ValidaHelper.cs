using System.Text.RegularExpressions;

namespace DbMercado.CrossCutting.Helpers;

public static class ValidaHelper
{

    /// <summary>
    /// Valida se um cpf é válido
    /// </summary>
    /// <param name="cpf">Cpf a ser validado</param>
    /// <returns>True para cpf válido e false para cpf inválido.</returns>
    /// 
    public static bool ValidaCpf(string cpf)
    {
        if (string.IsNullOrEmpty(cpf)) return false;

        var factory = Task<bool>.Factory;

        if (cpf.Length > 11)
            return false;
        while (cpf.Length != 11)
            cpf = '0' + cpf;
        bool igual = true;
        for (int i = 1; i < 11 && igual; i++)
            if (cpf[i] != cpf[0])
                igual = false;
        if (igual || cpf == "12345678909")
            return false;
        int[] numeros = new int[11];
        for (int i = 0; i < 11; i++)
            numeros[i] = int.Parse(cpf[i].ToString());
        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += (10 - i) * numeros[i];
        int resultado = soma % 11;
        if (resultado == 1 || resultado == 0)
        {
            if (numeros[9] != 0)
                return false;
        }
        else if (numeros[9] != 11 - resultado)
            return false;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += (11 - i) * numeros[i];
        resultado = soma % 11;
        if (resultado == 1 || resultado == 0)
        {
            if (numeros[10] != 0)
                return false;
        }
        else
            if (numeros[10] != 11 - resultado)
            return false;

        return true;
    }


    public static bool ValidaCnpj(string vrCNPJ)
    {
        if (string.IsNullOrEmpty(vrCNPJ)) return false;

        var factory = Task<bool>.Factory;

        string CNPJ = vrCNPJ.Replace(".", "");
        CNPJ = CNPJ.Replace("/", "");
        CNPJ = CNPJ.Replace("-", "");

        int[] digitos, soma, resultado;
        int nrDig;
        string ftmt;
        bool[] CNPJOk;

        ftmt = "6543298765432";
        digitos = new int[14];
        soma = new int[2];
        soma[0] = 0;
        soma[1] = 0;
        resultado = new int[2];
        resultado[0] = 0;
        resultado[1] = 0;
        CNPJOk = new bool[2];
        CNPJOk[0] = false;
        CNPJOk[1] = false;

        try
        {
            for (nrDig = 0; nrDig < 14; nrDig++)
            {
                digitos[nrDig] = int.Parse(
                 CNPJ.Substring(nrDig, 1));
                if (nrDig <= 11)
                    soma[0] += (digitos[nrDig] *
                    int.Parse(ftmt.Substring(
                      nrDig + 1, 1)));
                if (nrDig <= 12)
                    soma[1] += (digitos[nrDig] *
                    int.Parse(ftmt.Substring(
                      nrDig, 1)));
            }

            for (nrDig = 0; nrDig < 2; nrDig++)
            {
                resultado[nrDig] = (soma[nrDig] % 11);
                if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                    CNPJOk[nrDig] = (
                    digitos[12 + nrDig] == 0);

                else
                    CNPJOk[nrDig] = (
                    digitos[12 + nrDig] == (
                    11 - resultado[nrDig]));

            }

            return CNPJOk[0] && CNPJOk[1];

        }
        catch
        {
            return false;
        }

    }


    /// <summary>
    /// Valida cartão de crédito.
    /// </summary>
    /// <param name="NumeroCartao">Número do cartão - somente números.</param>
    /// <returns>bool.</returns>
    /// 
    public static bool ValidaCartaoDeCredito(string numeroCartao)
    {
        var factory = Task<bool>.Factory;

        int[] DELTAS = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
        int checksum = 0;
        char[] chars = numeroCartao.ToCharArray();

        for (int i = chars.Length - 1; i > -1; i--)
        {
            int j = ((int)chars[i]) - 48;
            checksum += j;
            if (((i - chars.Length) % 2) == 0)
                checksum += DELTAS[j];
        }

        return ((checksum % 10) == 0);
    }


    /// <summary>
    /// Valida email.
    /// </summary>
    /// <param name="email">Email.</param>
    /// <returns>bool.</returns>
    /// 
    public static bool Valida_Email(string email)
    {
        var factory = Task<bool>.Factory;
        return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    }

}
