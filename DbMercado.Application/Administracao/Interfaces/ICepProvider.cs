using DbMercado.Application.Administracao.Dtos.CEP;

namespace DbMercado.Application.Administracao.Interfaces;

public interface ICepProvider
{
    Task<CepResponse?> BuscarCepAsync(string cep);
}
