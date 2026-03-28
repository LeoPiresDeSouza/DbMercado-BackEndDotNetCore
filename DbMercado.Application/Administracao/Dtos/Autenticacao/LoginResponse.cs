namespace DbMercado.Application.Administracao.Dtos.Autenticacao;

public record LoginResponse(
    string Token,
    DateTime Expiration,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);
