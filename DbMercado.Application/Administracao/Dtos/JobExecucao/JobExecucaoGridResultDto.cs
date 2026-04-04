namespace DbMercado.Application.Administracao.Dtos.JobExecucao;

public class JobExecucaoGridResultDto
{
    public List<JobExecucaoGridRowDto> Rows { get; set; } = [];

    public int RowCount { get; set; }
}

public class JobExecucaoGridRowDto
{
    public long Id { get; set; }

    public string FireInstanceId { get; set; } = string.Empty;

    public string JobNome { get; set; } = string.Empty;

    public string JobGrupo { get; set; } = string.Empty;

    public string TriggerNome { get; set; } = string.Empty;

    public string TriggerGrupo { get; set; } = string.Empty;

    public DateTimeOffset InicioUtc { get; set; }

    public DateTimeOffset? FimUtc { get; set; }

    public long? DuracaoMs { get; set; }

    public bool? Sucesso { get; set; }

    public string? MensagemErro { get; set; }
}
