namespace DbMercado.Infrastructure.Jobs.Configuration;

/// <summary>
/// Configuração raiz do agendador Quartz (appsettings: Quartz).
/// </summary>
public sealed class QuartzSchedulingOptions
{
    public const string SectionName = "Quartz";

    /// <summary>Nome da instância do scheduler (visível em logs/diagnóstico).</summary>
    public string SchedulerName { get; set; } = "DbMercadoScheduler";

    /// <summary>Aguarda jobs em execução ao desligar o host.</summary>
    public bool WaitForJobsToComplete { get; set; } = true;

    /// <summary>Inicia o scheduler somente após a aplicação concluir o startup (migrations/seed antes dos jobs).</summary>
    public bool AwaitApplicationStarted { get; set; } = true;

    /// <summary>Atraso opcional após o startup antes do primeiro disparo do scheduler.</summary>
    public int StartDelayedSeconds { get; set; }

    public LogCleanupJobOptions LogCleanup { get; set; } = new();

    /// <summary>Remove arquivos e registros de m upload temporário não associado ao produto.</summary>
    public LimpezaMidiasTemporariasJobOptions LimpezaMidiasTemporarias { get; set; } = new();

    /// <summary>Reservado: sincronização com marketplaces externos.</summary>
    public JobScheduleOptions MarketplaceSync { get; set; } = new();

    /// <summary>Reservado: conciliação financeira.</summary>
    public JobScheduleOptions ConciliacaoFinanceira { get; set; } = new();

    /// <summary>Reservado: filas de reprocessamento.</summary>
    public JobScheduleOptions Reprocessamento { get; set; } = new();
}

/// <summary>Opções comuns a jobs agendados por cron.</summary>
public class JobScheduleOptions
{
    public bool Enabled { get; set; }

    /// <summary>
    /// Fallback da expressão cron Quartz (ex.: <c>0 0 3 * * ?</c> às 03:00 UTC) quando o parâmetro correspondente na tabela estiver ausente ou vazio.
    /// </summary>
    public string CronSchedule { get; set; } = "0 0 3 * * ?";
}

/// <summary>Limpeza de logs persistidos na tabela de aplicação (<c>dbAppLog</c>) via política em parâmetros Log/Limpeza.</summary>
public sealed class LogCleanupJobOptions : JobScheduleOptions
{
}

/// <summary>Mídias em <c>temporario</c> com <c>DataCriacao</c> anterior ao cutoff são excluídas.</summary>
public sealed class LimpezaMidiasTemporariasJobOptions : JobScheduleOptions
{
    /// <summary>Horas sem associação ao produto antes da remoção (padrão 24).</summary>
    public int HorasRetencao { get; set; } = 24;
}

/// <summary>Grupos Quartz para organizar jobs por área (facilita futuros UIs ou pausas por grupo).</summary>
public static class QuartzJobGroups
{
    public const string Maintenance = "Maintenance";
    public const string Marketplace = "Marketplace";
    public const string Financeiro = "Financeiro";
    public const string Reprocessamento = "Reprocessamento";
}
