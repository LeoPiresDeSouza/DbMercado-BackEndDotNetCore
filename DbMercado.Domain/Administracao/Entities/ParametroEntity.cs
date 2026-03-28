namespace DbMercado.Domain.Administracao.Entities;

/// <summary>
/// Entidade destinada a armazenar os parâmetros da aplicação.
/// </summary>
public class ParametroEntity : BaseEntity
{
    public long Id { get; set; }

    public string? Categoria { get; set; }

    public string? Atributo { get; set; }

    public string Chave { get; set; }

    public string Valor { get; set; }

    public string? Descricao { get; set; }

    #region Ctor

    public ParametroEntity()
    {
        Categoria = string.Empty;
        Atributo = string.Empty;
        Chave = string.Empty;
        Valor = string.Empty;
        Descricao = string.Empty;
    }

    #endregion Ctor
}
