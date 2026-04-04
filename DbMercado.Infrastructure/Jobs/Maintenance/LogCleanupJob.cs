using DbMercado.Application.Administracao.Interfaces;
using DbMercado.Infrastructure.Jobs.Abstractions;
using DbMercado.Infrastructure.Jobs.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace DbMercado.Infrastructure.Jobs.Maintenance;

/// <summary>
/// Executa a política de limpeza de <c>dbAppLog</c> (min/max registros + backup em disco), igual a <c>POST /api/app-logs/limpeza</c>.
/// </summary>
[DisallowConcurrentExecution]
public sealed class LogCleanupJob : ScopedBackgroundJob
{
    public const string JobName = "LogCleanup";

    private readonly IOptionsMonitor<QuartzSchedulingOptions> _quartzOptions;
    private readonly ILogger<LogCleanupJob> _logger;

    public LogCleanupJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<QuartzSchedulingOptions> quartzOptions,
        ILogger<LogCleanupJob> logger)
        : base(scopeFactory)
    {
        _quartzOptions = quartzOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(IJobExecutionContext context, IServiceProvider serviceProvider)
    {
        var opt = _quartzOptions.CurrentValue.LogCleanup;
        if (!opt.Enabled)
        {
            _logger.LogDebug("LogCleanupJob ignorado: LogCleanup.Enabled = false.");
            return;
        }

        var appLog = serviceProvider.GetRequiredService<IAppLogService>();
        var result = await appLog.ExecutarLimpezaAsync(context.CancellationToken);

        _logger.LogInformation(
            "LogCleanupJob: excluídos {Excluidos} registro(s). Backup: {Backup}.",
            result.RegistrosExcluidos,
            result.ArquivoBackup ?? "(nenhum)");
    }
}
