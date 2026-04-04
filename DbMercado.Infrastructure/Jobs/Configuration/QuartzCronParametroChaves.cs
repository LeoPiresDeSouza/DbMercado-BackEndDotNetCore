namespace DbMercado.Infrastructure.Jobs.Configuration;

/// <summary>
/// Categoria, atributo e chave na tabela de parâmetros para expressões cron dos jobs Quartz (UTC).
/// Valores carregados na inicialização da API; <see cref="QuartzSchedulingOptions"/> permanece como fallback.
/// </summary>
public static class QuartzCronParametroChaves
{
    public const string LogCleanupCategoria = "Log";
    public const string LogCleanupAtributo = "Limpeza";
    public const string LogCleanupChave = "Job";

    public const string LimpezaMidiasTemporariasCategoria = "Quartz";
    public const string LimpezaMidiasTemporariasAtributo = "LimpezaMidiasTemporarias";
    public const string LimpezaMidiasTemporariasChave = "CronSchedule";

    public const string MarketplaceSyncCategoria = "Quartz";
    public const string MarketplaceSyncAtributo = "MarketplaceSync";
    public const string MarketplaceSyncChave = "CronSchedule";

    public const string ConciliacaoFinanceiraCategoria = "Quartz";
    public const string ConciliacaoFinanceiraAtributo = "ConciliacaoFinanceira";
    public const string ConciliacaoFinanceiraChave = "CronSchedule";

    public const string ReprocessamentoCategoria = "Quartz";
    public const string ReprocessamentoAtributo = "Reprocessamento";
    public const string ReprocessamentoChave = "CronSchedule";
}
