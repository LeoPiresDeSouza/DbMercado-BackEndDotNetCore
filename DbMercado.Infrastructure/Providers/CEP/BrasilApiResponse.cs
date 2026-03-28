using System.Text.Json.Serialization;

namespace DbMercado.Infrastructure.Models.CEP;

/// <summary>
/// Modelo de resposta retornado pela API BrasilAPI.
/// Representa diretamente o JSON retornado pelo serviço externo.
/// </summary>
public class BrasilApiResponse
{
    #region Propriedades

    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("neighborhood")]
    public string? Neighborhood { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("service")]
    public string? Service { get; set; }

    #endregion
}
