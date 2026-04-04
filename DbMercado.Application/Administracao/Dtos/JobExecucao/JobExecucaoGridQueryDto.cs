namespace DbMercado.Application.Administracao.Dtos.JobExecucao;

public class JobExecucaoGridQueryDto
{
    public int StartRow { get; set; }

    public int EndRow { get; set; }

    public List<JobExecucaoGridSortItemDto>? SortModel { get; set; }

    public DateTimeOffset? DataInicio { get; set; }

    public DateTimeOffset? DataFim { get; set; }

    /// <summary>Filtro parcial por nome do job (contains, case-insensitive no SQL Server CI).</summary>
    public string? JobNome { get; set; }

    /// <summary><c>todos</c>, <c>sucesso</c>, <c>falha</c> ou <c>emAndamento</c>.</summary>
    public string? ResultadoFiltro { get; set; }
}

public class JobExecucaoGridSortItemDto
{
    public string ColId { get; set; } = string.Empty;

    public string? Sort { get; set; }
}
