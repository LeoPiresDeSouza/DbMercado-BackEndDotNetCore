namespace DbMercado.Domain.Administracao.Entities;

/// <summary>
/// Entidade que mapeia as propriedades dos módulos da aplicação.
/// </summary>
public class ModuloEntity : BaseEntity
{
    #region Propriedades

    public long Id { get; set; }

    public string NomeNormalizado { get; set; }

    public string NomeExibicao { get; set; }

    public string Descricao { get; set; }

    public int OrdemExibicao { get; set; }

    public string Icone { get; set; }

    public ICollection<FuncionalidadeEntity> Funcionalidades { get; set; } = new List<FuncionalidadeEntity>();

    #endregion Propriedades

    #region Ctor

    public ModuloEntity()
    {
        NomeNormalizado = string.Empty;
        NomeExibicao = string.Empty;
        Descricao = string.Empty;
        OrdemExibicao = 0;
        Icone = string.Empty;
    }

    #endregion Ctor

    #region regras de negócio
    #endregion regras de negócio
}
