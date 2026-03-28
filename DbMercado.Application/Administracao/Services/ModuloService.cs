using DbMercado.Application.Administracao.Dtos.Modulo;
using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Application.Administracao.Mappers.Modulo;
using DbMercado.Domain.Administracao.Interfaces.UnitsOfWork;

namespace DbMercado.Application.Administracao.Services;

#pragma warning disable

/// <summary>
/// Encapsula a lógica de negócio relacionada aos módulos, funcionalidades e permissões de acesso dos usuários.
/// </summary>
public class ModuloService: IModuloService
{

    #region Membros privados

    private readonly IUwAdministracao _uwAdministracao;

    #endregion Membros privados




    #region Propriedades públicas
    #endregion Propriedades públicas




    #region Ctor

    public ModuloService(IUwAdministracao uwAdministracao)
    {
        _uwAdministracao = uwAdministracao;
    }

    #endregion Ctor




    #region Métodos públicos

    /// <summary>
    /// Recupera a estrutura de acesso do usuário aos módulos, funcionalidades e permissões.
    /// O json é utilizado para montar o menu de acesso do usuário no frontend.
    /// </summary>
    /// <param name="usuario">Nome do usuário no identity, normalmente o email, para o qual se desje recuperar
    /// a estrutura de acessos</param>
    /// <returns>List<ModuloUsuarioResponse>?</returns>
    public async Task<List<ModuloUsuarioResponse>?> ModulosUsuarioAsync(string usuario)
    {
        // Recupero as claims do usuário a partir do identity, ou seja, os perfis e as permissões associadas a ele.

        var identityUser = await _uwAdministracao.UsuarioIdentityRepository.GetByNameAsync(usuario);
        var permissoes = await _uwAdministracao.UsuarioIdentityRepository.GetPermissoesAsync(identityUser);

        // Invoco o repositório de métodos do identity para autenticação e geração do token jwt.

        var modulos = await _uwAdministracao.ModuloRepository.ModulosUsuarioAsync(usuario, permissoes);

        // Preciso transformar a estrutura de dados retornada do banco de dados para o
        // formato esperado pelo frontend, ou seja, uma lista de ModuloUsuarioResponse.

        return modulos
                .Select(ModuloUsuarioMapper.ToUsuarioResponse)
                .ToList();
    }

    #endregion Métodos públicos
}
