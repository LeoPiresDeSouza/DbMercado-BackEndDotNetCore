using DbMercado.Application.Produto.Interfaces;
using DbMercado.Infrastructure.Jobs.Abstractions;
using DbMercado.Infrastructure.Jobs.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace DbMercado.Infrastructure.Jobs.Maintenance;

/// <summary>
/// Remove mídias com status temporário mais antigas que o período configurado (arquivo + registro).
/// </summary>
[DisallowConcurrentExecution]
public sealed class LimpezaMidiasTemporariasJob : ScopedBackgroundJob
{
    public const string JobName = "LimpezaMidiasTemporarias";

    private readonly IOptionsMonitor<QuartzSchedulingOptions> _quartzOptions;
    private readonly ILogger<LimpezaMidiasTemporariasJob> _logger;

    public LimpezaMidiasTemporariasJob(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<QuartzSchedulingOptions> quartzOptions,
        ILogger<LimpezaMidiasTemporariasJob> logger)
        : base(scopeFactory)
    {
        _quartzOptions = quartzOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(IJobExecutionContext context, IServiceProvider serviceProvider)
    {
        var opt = _quartzOptions.CurrentValue.LimpezaMidiasTemporarias;
        if (!opt.Enabled)
        {
            _logger.LogDebug("LimpezaMidiasTemporariasJob ignorado: LimpezaMidiasTemporarias.Enabled = false.");
            return;
        }

        var horas = Math.Clamp(opt.HorasRetencao, 1, 168);
        var midiaService = serviceProvider.GetRequiredService<IMidiaService>();
        await midiaService.ExcluirTemporariasExpiradasAsync(horas, context.CancellationToken);
        _logger.LogInformation(
            "LimpezaMidiasTemporariasJob concluído (retencao {Horas} h).",
            horas);
    }
}
