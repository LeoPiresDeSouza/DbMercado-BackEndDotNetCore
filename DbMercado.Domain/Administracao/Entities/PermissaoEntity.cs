using System.Text.Json.Serialization;

namespace DbMercado.Domain.Administracao.Entities;

/// <summary>
/// Entidade que mapeia as propriedades das pemissões que podem ser associadas a cada funcionalidade e
/// que controlarão o nível de acesso do usuário a cada funcionalidade da aplicação.
/// </summary>
public class PermissaoEntity : BaseEntity
{
    #region Propriedades

    public long Id { get; set; }

    public string Permissao { get; set; }

    public long FuncionalidadeId { get; set; }

    [JsonIgnore]
    public virtual FuncionalidadeEntity Funcionalidade { get; set; } = null!;

    #endregion Propriedades

    #region Ctor

    public PermissaoEntity()
    {
        Permissao = string.Empty;
        FuncionalidadeId = 0;
    }

    #endregion Ctor

    #region regras de negócio
    #endregion regras de negócio
}
