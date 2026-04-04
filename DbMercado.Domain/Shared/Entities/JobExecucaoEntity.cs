namespace DbMercado.Domain.Shared.Entities;

/// <summary>
/// Uma execução concreta de um job Quartz (início/fim, duração e resultado).
/// </summary>
public sealed class JobExecucaoEntity
{
    public long Id { get; set; }

    /// <summary>Identificador único do disparo no Quartz (<see cref="Quartz.IJobExecutionContext.FireInstanceId"/>).</summary>
    public string FireInstanceId { get; set; } = string.Empty;

    public string JobNome { get; set; } = string.Empty;

    public string JobGrupo { get; set; } = string.Empty;

    public string TriggerNome { get; set; } = string.Empty;

    public string TriggerGrupo { get; set; } = string.Empty;

    public DateTimeOffset InicioUtc { get; set; }

    public DateTimeOffset? FimUtc { get; set; }

    /// <summary>Duração informada pelo Quartz (<see cref="Quartz.IJobExecutionContext.JobRunTime"/>).</summary>
    public long? DuracaoMs { get; set; }

    public bool? Sucesso { get; set; }

    /// <summary>Resumo da falha (ex.: <see cref="Quartz.JobExecutionException.Message"/>).</summary>
    public string? MensagemErro { get; set; }
}
