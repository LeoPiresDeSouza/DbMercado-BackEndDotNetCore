using System.Reflection;

namespace DbMercado.CrossCutting.Helpers;

public static class ReflectionHelper
{
    /// <summary>
    /// Intera sobre todas as propriedades de um objeto de uma classe genérica
    /// e devolve uma string concatenando o nome da propriedade com o seu valor.
    /// Pode ser utilizada para gravar no log da aplicação os valores recebidos em um objeto no parâmetro do método.
    /// Exemplo: "Nome:Leo Souza - Matricula:3061 - UsuarioAd:leo.souza"
    /// </summary>
    /// <typeparam name="T">Tipo da classe correspondente ao objeto enviado no parâmetro.</typeparam>
    /// <param name="obj">Instância da classe na qual se deseja interar.</param>
    /// <returns>String com o nome e os valores das propriedades encontradas.</returns>
    public static string? GetPropertiesKeyValuesAsString<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        string txtProperties = string.Empty;

        var properties = obj.GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            var pValue = property.GetValue(obj);
            string? sValue = pValue == null ? null : pValue.ToString();
            string sName = property.Name;

            if (sValue != null)
            {
                txtProperties = $"{txtProperties} - {sName}:{sValue}";
            }
        }

        return txtProperties;
    }



    /// <summary>
    /// Iintera sobre todas as propriedades de um objeto de uma classe genérica
    /// e devolve uma string concatenando os valores de cada uma.
    /// Pode ser utilizada para para gerar chaves de cache a partir de um objeto recebido no parâmetro do método.
    /// Exemplo: "Leo Souza:3061:leo.souza"
    /// </summary>
    /// <typeparam name="T">Tipo da classe correspondente ao objeto enviado no parâmetro.</typeparam>
    /// <param name="obj">Instância da classe na qual se deseja interar.</param>
    /// <returns>String com o nome e os valores das propriedades encontradas.</returns>
    public static string GetPropertiesValuesAsString<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        string txtProperties = string.Empty;

        var properties = obj.GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            var pValue = property.GetValue(obj);
            string? sValue = pValue == null ? null : pValue.ToString();
            string sName = property.Name;

            if (sValue != null)
            {
                txtProperties = $"{txtProperties}:{sValue}";
            }
        }

        return txtProperties;
    }



    /// <summary>
    /// Intera sobre todas as propriedades de um objeto de uma classe genérica
    /// e devolve uma string JSON concatenando os valores de cada uma.
    /// Pode ser utilizada para para gerar chaves de cache a partir de um objeto recebido no parâmetro do método.
    /// Exemplo: "Leo Souza:3061:leo.souza"
    /// </summary>
    /// <typeparam name="T">Tipo da classe correspondente ao objeto enviado no parâmetro.</typeparam>
    /// <param name="obj">Instância da classe na qual se deseja interar.</param>
    /// <returns>String com o nome e os valores das propriedades encontradas.</returns>
    public static string GetPropertiesValuesAsJson<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));

        var jsonText = System.Text.Json.JsonSerializer.Serialize(obj);
        return jsonText;

    }
}
