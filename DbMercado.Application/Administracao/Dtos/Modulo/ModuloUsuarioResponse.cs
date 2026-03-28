namespace DbMercado.Application.Administracao.Dtos.Modulo;

public record ModuloUsuarioResponse
{
    #region Propriedades ModuloUsuarioResponse

    public long ModuloId{ get; set; }
    public string NomeNormalizado { get; set; } = string.Empty;
    public string NomeExibicao { get; set; } = string.Empty;
    public int OrdemExibicao { get; set; } = 0;
    public string Icone { get; set; } = string.Empty;

    public List<FuncionalidadeUsuarioResponse> Funcionalidades{ get; set; } = new ();

    #endregion Propriedades ModuloUsuarioResponse
}



public record FuncionalidadeUsuarioResponse
{
    #region Propriedades FuncionalidadesUsuarioResponse

    public long FuncionalidadeId { get; set; }
    public string NomeNormalizado { get; set; } = string.Empty;
    public string NomeExibicao { get; set; } = string.Empty;
    public int OrdemExibicao { get; set; }
    public string Icone { get; set; } = string.Empty;
    public List<PermissaoUsuarioResponse> Permissoes { get; set; } = new ();

    #endregion Propriedades FuncionalidadesUsuarioResponse

}



public record PermissaoUsuarioResponse
{
    #region Propriedades PermissaoUsuarioResponse

    public long PermissaoId { get; set; }
    public string Permissao { get; set; } = string.Empty;

    #endregion Propriedades PermissaoUsuarioResponse

}
