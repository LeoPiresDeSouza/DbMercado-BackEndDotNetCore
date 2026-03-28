using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DbMercado.Infrastructure.Jobs.Abstractions;

/// <summary>
/// Job com escopo de DI por execução (adequado a <see cref="Microsoft.EntityFrameworkCore.DbContext"/> e serviços scoped).
/// </summary>
public abstract class ScopedBackgroundJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    protected ScopedBackgroundJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        await ExecuteAsync(context, scope.ServiceProvider);
    }

    protected abstract Task ExecuteAsync(IJobExecutionContext context, IServiceProvider serviceProvider);
}
