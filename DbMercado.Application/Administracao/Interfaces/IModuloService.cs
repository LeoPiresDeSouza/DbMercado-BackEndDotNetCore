using DbMercado.Application.Administracao.Dtos.Modulo;

namespace DbMercado.Application.Administracao.Interfaces;

/// <summary>
/// Encapsula a lógica de negócio relacionada aos módulos, funcionalidades e permissões de acesso dos usuários.
/// </summary>
/// 
public interface IModuloService
{
    /// <summary>
    /// Recupera a estrutura de acesso do usuário aos módulos, funcionalidades e permissões.
    /// O json é utilizado para montar o menu de acesso do usuário no frontend.
    /// </summary>
    /// <param name="usuario">Nome do usuário no identity, normalmente o email, para o qual se desje recuperar
    /// a estrutura de acessos</param>
    /// <returns>List<ModuloUsuarioResponse>?</returns>
    Task<List<ModuloUsuarioResponse>?> ModulosUsuarioAsync(string usuario);
}
