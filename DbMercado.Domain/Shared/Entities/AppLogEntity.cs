using System.ComponentModel.DataAnnotations.Schema;

namespace DbMercado.Domain.Shared.Entities;

/// <summary>
/// Registro de log persistido no banco para auditoria e rastreio (scope + state do ILogger).
/// </summary>
public class AppLogEntity
{
    public long Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? ErrorCode { get; set; }
    public string? TraceId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
