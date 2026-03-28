using DbMercado.Infrastructure.Providers.Logging;

namespace DbMercado.Api.Extensions;

public static class LoggingAppExtensions
{
    /// <summary>
    /// Adiciona o DatabaseLoggerProvider ao LoggerFactory para persistir logs no banco.
    /// Chamar após builder.Build(). Requer que DatabaseLoggerProvider esteja registrado no DI (AddSingleton).
    /// </summary>
    public static WebApplication UseDatabaseLogger(this WebApplication app)
    {
        var loggerFactory = (LoggerFactory)app.Services.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddProvider(app.Services.GetRequiredService<DatabaseLoggerProvider>());
        return app;
    }
}
