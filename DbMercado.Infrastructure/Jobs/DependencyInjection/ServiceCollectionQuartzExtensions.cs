using DbMercado.Infrastructure.Jobs.Configuration;
using DbMercado.Infrastructure.Jobs.Maintenance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DbMercado.Infrastructure.Jobs.DependencyInjection;

public static class ServiceCollectionQuartzExtensions
{
    /// <summary>
    /// Registra Quartz, jobs habilitados em <see cref="QuartzSchedulingOptions.SectionName"/> e o hosted service do scheduler.
    /// </summary>
    public static IServiceCollection AddDbMercadoQuartz(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(QuartzSchedulingOptions.SectionName);
        services.Configure<QuartzSchedulingOptions>(section);

        var options = section.Get<QuartzSchedulingOptions>() ?? new QuartzSchedulingOptions();

        services.AddQuartz(quartzConfigurator =>
        {
            quartzConfigurator.SchedulerName = options.SchedulerName;

            RegisterLogCleanupIfEnabled(quartzConfigurator, options);

            // Futuros: registrar aqui MarketplaceSyncJob, ConciliacaoFinanceiraJob, ReprocessamentoJob
            // usando options.MarketplaceSync / ConciliacaoFinanceira / Reprocessamento e os grupos em QuartzJobGroups.
        });

        services.AddQuartzHostedService(hostOptions =>
        {
            hostOptions.WaitForJobsToComplete = options.WaitForJobsToComplete;
            hostOptions.AwaitApplicationStarted = options.AwaitApplicationStarted;
            if (options.StartDelayedSeconds > 0)
                hostOptions.StartDelay = TimeSpan.FromSeconds(options.StartDelayedSeconds);
        });

        return services;
    }

    private static void RegisterLogCleanupIfEnabled(IServiceCollectionQuartzConfigurator q, QuartzSchedulingOptions options)
    {
        if (!options.LogCleanup.Enabled)
            return;

        var cron = options.LogCleanup.CronSchedule?.Trim();
        if (string.IsNullOrEmpty(cron))
            throw new InvalidOperationException("Quartz:LogCleanup:CronSchedule é obrigatório quando LogCleanup.Enabled é true.");

        var jobKey = new JobKey(LogCleanupJob.JobName, QuartzJobGroups.Maintenance);
        var triggerKey = new TriggerKey($"{LogCleanupJob.JobName}-trigger", QuartzJobGroups.Maintenance);

        q.AddJob<LogCleanupJob>(j => j.WithIdentity(jobKey));
        q.AddTrigger(t => t
            .ForJob(jobKey)
            .WithIdentity(triggerKey)
            .WithCronSchedule(cron, b => b.InTimeZone(TimeZoneInfo.Utc)));
    }
}
