namespace DbMercado.Application.Produto.Dtos;

public sealed class MidiaUploadResponseDto
{
    public long MidiaId { get; init; }

    public string Url { get; init; } = string.Empty;

    public string? ThumbnailUrl { get; init; }
}
