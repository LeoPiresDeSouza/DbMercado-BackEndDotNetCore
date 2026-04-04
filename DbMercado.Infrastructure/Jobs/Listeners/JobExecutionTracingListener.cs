using DbMercado.Domain.Shared.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DbMercado.Infrastructure.Jobs.Listeners;

/// <summary>
/// Grava início e fim de cada execução de job em <c>dbJobExecucao</c>.
/// </summary>
public sealed class JobExecutionTracingListener : IJobListener
{
    public const string ListenerName = "DbMercadoJobExecutionTracing";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobExecutionTracingListener> _logger;

    public JobExecutionTracingListener(
        IServiceScopeFactory scopeFactory,
        ILogger<JobExecutionTracingListener> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public string Name => ListenerName;

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobExecucaoRepository>();
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;
            await repo.RegistrarInicioAsync(
                context.FireInstanceId,
                jobKey.Name,
                jobKey.Group,
                triggerKey.Name,
                triggerKey.Group,
                DateTimeOffset.UtcNow,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao registrar início de execução do job {Job}.", context.JobDetail.Key);
        }
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobExecucaoRepository>();
            var duracaoMs = (long)Math.Round(context.JobRunTime.TotalMilliseconds);
            var sucesso = jobException is null;
            var mensagem = sucesso ? null : jobException!.InnerException?.Message ?? jobException.Message;
            await repo.FinalizarAsync(
                context.FireInstanceId,
                DateTimeOffset.UtcNow,
                duracaoMs,
                sucesso,
                mensagem,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao registrar fim de execução do job {Job}.", context.JobDetail.Key);
        }
    }
}
