namespace DbMercado.Application.Produto.Dtos;

public sealed class MidiaResponseDto
{
    public long Id { get; init; }

    public string Url { get; init; } = string.Empty;

    public string? ThumbnailUrl { get; init; }

    public string Tipo { get; init; } = string.Empty;

    public int Ordem { get; init; }

    public bool IsPrincipal { get; init; }

    public decimal? Duracao { get; init; }

    public string Status { get; init; } = string.Empty;
}
