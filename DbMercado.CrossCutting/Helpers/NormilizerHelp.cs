namespace DbMercado.CrossCutting.Helpers;

public static class CepNormalizer
{
    public static string Normalize(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return string.Empty;

        return new string(cep.Where(char.IsDigit).ToArray());
    }
}
