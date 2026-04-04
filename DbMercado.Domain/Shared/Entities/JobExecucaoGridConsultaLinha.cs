namespace DbMercado.Domain.Shared.Entities;

/// <summary>Linha do grid de execuções de jobs Quartz.</summary>
public sealed class JobExecucaoGridConsultaLinha
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
