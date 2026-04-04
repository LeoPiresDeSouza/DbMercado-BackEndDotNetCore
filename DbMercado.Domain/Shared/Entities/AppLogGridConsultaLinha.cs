namespace DbMercado.Domain.Shared.Entities;

/// <summary>
/// Linha da consulta de grid de logs — sem materializar <c>Exception</c> (LOB) no SELECT.
/// </summary>
public sealed class AppLogGridConsultaLinha
{
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string Level { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool HasException { get; set; }

    public string? UserName { get; set; }

    public string? Path { get; set; }

    public string? Method { get; set; }
}
