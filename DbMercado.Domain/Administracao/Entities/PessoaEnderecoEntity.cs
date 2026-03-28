namespace DbMercado.Domain.Administracao.Entities;

public class PessoaEnderecoEntity : BaseEntity
{
    public long Id { get; set; }

    public long PessoaId { get; set; }

    public string TipoLogradouro { get; set; }

    public string Logradouro { get; set; }

    public string Numero { get; set; }

    public string Complemento { get; set; }

    public string Bairro { get; set; }

    public string Cidade { get; set; }

    public string Estado { get; set; }

    public string Pais { get; set; }

    public string Cep { get; set; }

    public PessoaEntity Pessoa { get; set; } = null!;

    public PessoaEnderecoEntity()
    {
        TipoLogradouro = string.Empty;
        Logradouro = string.Empty;
        Numero = string.Empty;
        Complemento = string.Empty;
        Bairro = string.Empty;
        Cidade = string.Empty;
        Estado = string.Empty;
        Pais = "Brasil";
        Cep = string.Empty;
    }
}
