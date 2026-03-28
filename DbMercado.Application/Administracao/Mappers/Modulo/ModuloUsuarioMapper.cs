using DbMercado.Application.Administracao.Dtos.Modulo;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Application.Administracao.Mappers.Modulo;

public static class ModuloUsuarioMapper
{
    public static ModuloUsuarioResponse ToUsuarioResponse(ModuloEntity modulo)
    {
        return new ModuloUsuarioResponse
        {
            ModuloId = modulo.Id,
            NomeNormalizado = modulo.NomeNormalizado,
            NomeExibicao = modulo.NomeExibicao,
            OrdemExibicao = modulo.OrdemExibicao,
            Icone = modulo.Icone,

            Funcionalidades = modulo.Funcionalidades
                .Select(FuncionalidadeUsuarioMapper.ToUsuarioResponse)
                .ToList()
        };
    }


    public static List<ModuloUsuarioResponse> MapToListUsuarioResponse(IEnumerable<ModuloEntity> modulos)
    {
        return modulos.Select(ToUsuarioResponse).ToList();
    }
}
