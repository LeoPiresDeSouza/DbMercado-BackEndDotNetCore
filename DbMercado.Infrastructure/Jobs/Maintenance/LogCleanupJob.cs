using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Jobs.Abstractions;
using DbMercado.Infrastructure.Jobs.Configuration;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace DbMercado.Infrastructure.Jobs.Maintenance;

/// <summary>
/// Remove registros antigos da tabela de log da aplicação (<c>dbAppLog</c>).
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

        var retentionDays = Math.Clamp(opt.RetentionDays, 1, 3650);
        var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

        var db = serviceProvider.GetRequiredService<AppDbContext>();

        var deleted = await db.Set<AppLogEntity>()
            .Where(e => e.CreatedAt < cutoff)
            .ExecuteDeleteAsync(context.CancellationToken);

        _logger.LogInformation(
            "LogCleanupJob: removidos {Deleted} registro(s) com CreatedAt anterior a {Cutoff:O} (retenção {RetentionDays} dia(s)).",
            deleted,
            cutoff,
            retentionDays);
    }
}
