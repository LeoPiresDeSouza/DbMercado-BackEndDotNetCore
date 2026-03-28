namespace DbMercado.Application.Administracao.Dtos.CEP;

public record EnderecoDto
{
    public string Cep { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    public long? MunicipioId { get; set; }

    public string Pais { get; set; } = "Brasil";
}
