using DbMercado.Application.Administracao.Dtos.Modulo;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Application.Administracao.Mappers.Modulo;

public static class FuncionalidadeUsuarioMapper
{
    public static FuncionalidadeUsuarioResponse ToUsuarioResponse(FuncionalidadeEntity funcionalidade)
    {
        return new FuncionalidadeUsuarioResponse
        {
            FuncionalidadeId = funcionalidade.Id,
            NomeNormalizado = funcionalidade.NomeNormalizado,
            NomeExibicao = funcionalidade.NomeExibicao,
            OrdemExibicao = funcionalidade.OrdemExibicao,
            Icone = funcionalidade.Icone,

            Permissoes = funcionalidade.Permissoes
                .Select(PermissaoUsuarioMapper.ToUsuarioResponse)
                .ToList()
        };
    }
}
