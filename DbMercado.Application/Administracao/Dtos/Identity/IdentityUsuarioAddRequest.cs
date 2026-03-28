namespace DbMercado.Application.Administracao.Dtos.Identity;

public record IdentityUsuarioAddRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
