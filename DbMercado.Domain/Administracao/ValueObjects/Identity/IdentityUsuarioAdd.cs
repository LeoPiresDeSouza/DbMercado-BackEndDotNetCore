namespace DbMercado.Domain.Administracao.ValueObjects.Identity;

public record IdentityUsuarioAdd
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
