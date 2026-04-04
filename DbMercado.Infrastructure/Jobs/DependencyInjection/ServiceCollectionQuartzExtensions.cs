using DbMercado.Infrastructure.Jobs.Configuration;
using DbMercado.Infrastructure.Jobs.Listeners;
using DbMercado.Infrastructure.Jobs.Maintenance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.Matchers;

namespace DbMercado.Infrastructure.Jobs.DependencyInjection;

public static class ServiceCollectionQuartzExtensions
{
    /// <summary>
    /// Registra Quartz, jobs habilitados em <see cref="QuartzSchedulingOptions.SectionName"/> e o hosted service do scheduler.
    /// As expressões cron efetivas vêm da tabela <c>dbParametro</c> quando preenchidas; caso contrário usa-se <c>CronSchedule</c> do appsettings.
    /// </summary>
    public static IServiceCollection AddDbMercadoQuartz(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(QuartzSchedulingOptions.SectionName);
        services.Configure<QuartzSchedulingOptions>(section);

        var optionsSnapshot = section.Get<QuartzSchedulingOptions>() ?? new QuartzSchedulingOptions();

        services.AddSingleton<JobExecutionTracingListener>();

        services.AddQuartz(quartzConfigurator =>
        {
            quartzConfigurator.SchedulerName = optionsSnapshot.SchedulerName;

            quartzConfigurator.AddJobListener<JobExecutionTracingListener>(EverythingMatcher<JobKey>.AllJobs());

            RegisterLogCleanupIfEnabled(quartzConfigurator, optionsSnapshot, configuration);
            RegisterLimpezaMidiasTemporariasIfEnabled(quartzConfigurator, optionsSnapshot, configuration);

            // Futuros: registrar aqui MarketplaceSyncJob, ConciliacaoFinanceiraJob, ReprocessamentoJob
            // usando options e QuartzCronParametroChaves (parâmetros já previstos no seed).
        });

        services.AddQuartzHostedService(hostOptions =>
        {
            hostOptions.WaitForJobsToComplete = optionsSnapshot.WaitForJobsToComplete;
            hostOptions.AwaitApplicationStarted = optionsSnapshot.AwaitApplicationStarted;
            if (optionsSnapshot.StartDelayedSeconds > 0)
                hostOptions.StartDelay = TimeSpan.FromSeconds(optionsSnapshot.StartDelayedSeconds);
        });

        return services;
    }

    private static void RegisterLogCleanupIfEnabled(
        IServiceCollectionQuartzConfigurator q,
        QuartzSchedulingOptions options,
        IConfiguration configuration)
    {
        if (!options.LogCleanup.Enabled)
            return;

        var cron = ResolverCron(
            configuration,
            QuartzCronParametroChaves.LogCleanupCategoria,
            QuartzCronParametroChaves.LogCleanupAtributo,
            QuartzCronParametroChaves.LogCleanupChave,
            options.LogCleanup.CronSchedule,
            "LogCleanup");

        var jobKey = new JobKey(LogCleanupJob.JobName, QuartzJobGroups.Maintenance);
        var triggerKey = new TriggerKey($"{LogCleanupJob.JobName}-trigger", QuartzJobGroups.Maintenance);

        q.AddJob<LogCleanupJob>(j => j.WithIdentity(jobKey));
        q.AddTrigger(t => t
            .ForJob(jobKey)
            .WithIdentity(triggerKey)
            .WithCronSchedule(cron, b => b.InTimeZone(TimeZoneInfo.Utc)));
    }

    private static void RegisterLimpezaMidiasTemporariasIfEnabled(
        IServiceCollectionQuartzConfigurator q,
        QuartzSchedulingOptions options,
        IConfiguration configuration)
    {
        if (!options.LimpezaMidiasTemporarias.Enabled)
            return;

        var cron = ResolverCron(
            configuration,
            QuartzCronParametroChaves.LimpezaMidiasTemporariasCategoria,
            QuartzCronParametroChaves.LimpezaMidiasTemporariasAtributo,
            QuartzCronParametroChaves.LimpezaMidiasTemporariasChave,
            options.LimpezaMidiasTemporarias.CronSchedule,
            "LimpezaMidiasTemporarias");

        var jobKey = new JobKey(LimpezaMidiasTemporariasJob.JobName, QuartzJobGroups.Maintenance);
        var triggerKey = new TriggerKey($"{LimpezaMidiasTemporariasJob.JobName}-trigger", QuartzJobGroups.Maintenance);

        q.AddJob<LimpezaMidiasTemporariasJob>(j => j.WithIdentity(jobKey));
        q.AddTrigger(t => t
            .ForJob(jobKey)
            .WithIdentity(triggerKey)
            .WithCronSchedule(cron, b => b.InTimeZone(TimeZoneInfo.Utc)));
    }

    private static string ResolverCron(
        IConfiguration configuration,
        string categoria,
        string atributo,
        string chave,
        string? fallbackAppsettings,
        string nomeJobParaErro)
    {
        var fromDb = QuartzCronParametroLeitor.ObterValorCron(configuration, categoria, atributo, chave);

        var cron = !string.IsNullOrWhiteSpace(fromDb)
            ? fromDb!.Trim()
            : fallbackAppsettings?.Trim();

        if (string.IsNullOrEmpty(cron))
        {
            throw new InvalidOperationException(
                $"Quartz:{nomeJobParaErro}: defina a expressão cron na tabela dbParametro ({categoria}/{atributo}/{chave}) " +
                $"ou em appsettings ({QuartzSchedulingOptions.SectionName}:{nomeJobParaErro}:CronSchedule) quando o job estiver habilitado.");
        }

        return cron;
    }
}
