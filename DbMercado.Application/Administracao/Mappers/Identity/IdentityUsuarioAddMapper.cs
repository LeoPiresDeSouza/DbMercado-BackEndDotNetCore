using DbMercado.Application.Administracao.Dtos.Identity;
using DbMercado.Domain.Administracao.ValueObjects.Identity;

namespace DbMercado.Application.Administracao.Mappers.Identity;

public static class IdentityUsuarioAddMapper
{
    public static IdentityUsuarioAdd ToIdentityUsuarioAdd(IdentityUsuarioAddRequest identityUser)
    {
        return new IdentityUsuarioAdd
        {
            Nome = identityUser.Nome,
            Email = identityUser.Email
        };
    }
}
