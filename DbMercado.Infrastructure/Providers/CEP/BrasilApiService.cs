using DbMercado.Application.Administracao.Dtos.CEP;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Infrastructure.Models.CEP;
using System.Net.Http.Json;

namespace DbMercado.Infrastructure.Providers.CEP;

public class BrasilApiService : ICepProvider
{
    private readonly IHttpClientFactory _factory;

    public BrasilApiService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<CepResponse?> BuscarCepAsync(string cep)
    {
        var client = _factory.CreateClient("BrasilApi");

        var response = await client.GetAsync($"{cep}");

        if (!response.IsSuccessStatusCode)
            return null;

        var data = await response.Content.ReadFromJsonAsync<BrasilApiResponse>();

        if (data == null)
            return null;

        return new CepResponse
        {
            Cep = data.Cep ?? string.Empty,
            Logradouro = data.Street ?? string.Empty,
            Bairro = data.Neighborhood ?? string.Empty,
            Cidade = data.City ?? string.Empty,
            Uf = data.State ?? string.Empty
        };
    }
}
