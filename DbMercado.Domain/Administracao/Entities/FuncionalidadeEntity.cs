using System.Text.Json.Serialization;

namespace DbMercado.Domain.Administracao.Entities;

/// <summary>
/// Entidade que mapeia as propriedades dos módulos da aplicação.
/// </summary>
public class FuncionalidadeEntity : BaseEntity
{
    #region Propriedades

    public long Id { get; set; }

    public string NomeNormalizado { get; set; }

    public string NomeExibicao { get; set; }

    public string Descricao { get; set; }

    public int OrdemExibicao { get; set; }

    public string Icone { get; set; }

    public long ModuloId { get; set; }

    [JsonIgnore]
    public virtual ModuloEntity Modulo { get; set; } = null!;

    public ICollection<PermissaoEntity> Permissoes { get; set; } = new List<PermissaoEntity>();

    #endregion Propriedades

    #region Ctor

    public FuncionalidadeEntity()
    {
        NomeNormalizado = string.Empty;
        NomeExibicao = string.Empty;
        Descricao = string.Empty;
        OrdemExibicao = 0;
        Icone = string.Empty;
        ModuloId = 0;
    }

    #endregion Ctor

    #region regras de negócio
    #endregion regras de negócio
}
