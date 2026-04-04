namespace DbMercado.Application.Administracao.Dtos.AppLog;

public class AppLogGridResultDto
{
    public List<AppLogGridRowDto> Rows { get; set; } = [];

    public int RowCount { get; set; }
}

public class AppLogGridRowDto
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
