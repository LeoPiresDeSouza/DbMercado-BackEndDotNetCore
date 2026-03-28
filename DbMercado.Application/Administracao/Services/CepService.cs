using DbMercado.Application.Administracao.Dtos.CEP;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.CrossCutting.Helpers;
using System.Security.Cryptography;


namespace DbMercado.Application.Administracao.Services;

public class CepService : ICepService
{
    private readonly ICepProvider _cepProvider;


    public CepService(ICepProvider cepProvider)
    {
        _cepProvider = cepProvider;

    }

    public async Task<EnderecoDto?> ConsultarCepAsync(string cep)
    {
        cep = CepNormalizer.Normalize(cep);

        if (cep.Length != 8)
            return null;

        var result = await _cepProvider.BuscarCepAsync(cep);

        if (result == null)
            return null;

        var endereco = new EnderecoDto
        {
            Cep = result.Cep,
            Logradouro = result.Logradouro,
            Bairro = result.Bairro,
            Cidade = result.Cidade,
            Uf = result.Uf
        };

        return endereco;
    }
}
