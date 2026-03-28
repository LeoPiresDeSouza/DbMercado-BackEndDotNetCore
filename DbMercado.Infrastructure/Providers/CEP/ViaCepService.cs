using DbMercado.Application.Administracao.Dtos.CEP;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Infrastructure.Models.CEP;
using System.Net.Http.Json;

namespace DbMercado.Infrastructure.Providers.CEP;

public class ViaCepService : ICepProvider
{
    private readonly IHttpClientFactory _factory;

    public ViaCepService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<CepResponse?> BuscarCepAsync(string cep)
    {
        var client = _factory.CreateClient("ViaCep");

        var response = await client.GetAsync($"{cep}/json/");

        if (!response.IsSuccessStatusCode)
            return null;

        var data = await response.Content.ReadFromJsonAsync<ViaCepResponse>();

        if (data == null) return null;
        if (data.Erro == null || (bool)data.Erro ) return null;

        return new CepResponse
        {
            Cep = data.Cep ?? string.Empty,
            Logradouro = data.Logradouro ?? string.Empty,
            Bairro = data.Bairro ?? string.Empty,
            Cidade = data.Localidade ?? string.Empty,
            Uf = data.Uf ?? string.Empty
        };
    }
}
