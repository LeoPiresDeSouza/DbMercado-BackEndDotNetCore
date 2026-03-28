namespace DbMercado.Application.Administracao.Dtos.Autenticacao;

/// <summary>
/// Request para renovar o token de acesso usando o refresh token
/// </summary>
public record RefreshTokenRequest(string RefreshToken);