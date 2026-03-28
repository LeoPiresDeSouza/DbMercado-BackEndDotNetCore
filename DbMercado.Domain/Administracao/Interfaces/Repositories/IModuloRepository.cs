using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Domain.Administracao.Interfaces.Repositories;

public interface IModuloRepository: IBaseRepository<ModuloEntity>
{
    #region Métodos públicos

    /// <summary>
    /// Retorna a lista de módulos associados a um usuário específico,
    /// juntamente com as funcionalidades correspondentes a cada módulo e as permissões
    /// associadas a cvada funcionalidade.
    /// Essa consulta é essencial para determinar quais módulos e funcionalidades um usuário tem acesso,
    /// permitindo uma gestão eficiente de permissões e personalização da interface de acordo com o perfil do usuário.
    /// </summary>
    /// <param name="usuario">nome do usuário no identity</param>
    /// <returns>List<ModuloEntity></returns>
    Task<List<ModuloEntity>> ModulosUsuarioAsync(string usuario, List<long> permissoesIds);

    #endregion Métodos públicos
}
