namespace DbMercado.Application.Administracao.Dtos.AppLog;

public class AppLogGridQueryDto
{
    public int StartRow { get; set; }

    public int EndRow { get; set; }

    public List<AppLogGridSortItemDto>? SortModel { get; set; }

    public DateTimeOffset? DataInicio { get; set; }

    public DateTimeOffset? DataFim { get; set; }

    public bool SomenteComExcecao { get; set; }

    public List<string>? Levels { get; set; }
}

public class AppLogGridSortItemDto
{
    public string ColId { get; set; } = string.Empty;

    public string? Sort { get; set; }
}
