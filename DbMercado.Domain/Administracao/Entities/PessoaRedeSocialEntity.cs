namespace DbMercado.Domain.Administracao.Entities;

public class PessoaRedeSocialEntity : BaseEntity
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public string TipoRedeSocial { get; set; }

    public string Perfil { get; set; }

    public PessoaEntity Pessoa { get; set; } = null!;

    public PessoaRedeSocialEntity()
    {
        TipoRedeSocial = string.Empty;
        Perfil = string.Empty;
    }
}
