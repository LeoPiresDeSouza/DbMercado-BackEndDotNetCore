using DbMercado.Application.Administracao.Dtos.CEP;
using DbMercado.Application.Administracao.Interfaces;

namespace DbMercado.Infrastructure.Providers.CEP;

public class CepFallbackCepService : ICepProvider
{
    private readonly IEnumerable<ICepProvider> _providers;

    public CepFallbackCepService(IEnumerable<ICepProvider> providers)
    {
        _providers = providers;
    }

    public async Task<CepResponse?> BuscarCepAsync(string cep)
    {
        foreach (var provider in _providers)
        {
            var result = await provider.BuscarCepAsync(cep);

            if (result != null)
                return result;
        }

        return null;
    }
}
