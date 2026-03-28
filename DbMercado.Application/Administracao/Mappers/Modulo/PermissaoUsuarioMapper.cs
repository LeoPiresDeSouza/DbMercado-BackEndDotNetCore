using DbMercado.Application.Administracao.Dtos.Modulo;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Application.Administracao.Mappers.Modulo;

public static class PermissaoUsuarioMapper
{
    public static PermissaoUsuarioResponse ToUsuarioResponse(PermissaoEntity permissao)
    {
        return new PermissaoUsuarioResponse
        {
            PermissaoId = permissao.Id,
            Permissao = permissao.Permissao
        };
    }
}
