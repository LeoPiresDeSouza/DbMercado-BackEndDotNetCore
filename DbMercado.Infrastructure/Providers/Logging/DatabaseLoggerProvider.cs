using System.Collections.Concurrent;
using DbMercado.Domain.Shared.Entities;
using DbMercado.Infrastructure.Shared.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbMercado.Infrastructure.Providers.Logging;

public sealed class DatabaseLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers = new();
    private IExternalScopeProvider? _scopeProvider;

    public DatabaseLoggerProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new DatabaseLogger(name, () => _scopeProvider, _scopeFactory));

    public void Dispose() => _loggers.Clear();

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}

public sealed class DatabaseLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Func<IExternalScopeProvider?> _scopeProviderAccessor;
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseLogger(
        string categoryName,
        Func<IExternalScopeProvider?> scopeProviderAccessor,
        IServiceScopeFactory scopeFactory)
    {
        _categoryName = categoryName;
        _scopeProviderAccessor = scopeProviderAccessor;
        _scopeFactory = scopeFactory;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
        _scopeProviderAccessor()?.Push(state) ?? NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        string? errorCode = null;

        if (state is IEnumerable<KeyValuePair<string, object>> structuredState)
        {
            foreach (var kv in structuredState)
            {
                if (string.Equals(kv.Key, "ErrorCode", StringComparison.OrdinalIgnoreCase))
                {
                    errorCode = kv.Value?.ToString();
                    break;
                }
            }
        }

        var scopeProvider = _scopeProviderAccessor();
        var scopeData = new Dictionary<string, object?>();

        scopeProvider?.ForEachScope((scope, dict) =>
        {
            if (scope is IEnumerable<KeyValuePair<string, object?>> kvps)
            {
                foreach (var kv in kvps)
                    dict[kv.Key] = kv.Value;
            }
        }, scopeData);

        scopeData.TryGetValue("TraceId", out var traceId);
        scopeData.TryGetValue("UserId", out var userId);
        scopeData.TryGetValue("UserName", out var userName);
        scopeData.TryGetValue("Path", out var path);
        scopeData.TryGetValue("Method", out var method);
        scopeData.TryGetValue("Ip", out var ip);
        scopeData.TryGetValue("UserAgent", out var userAgent);

        var entry = new AppLogEntity
        {
            Category = Truncate(_categoryName, 256),
            Level = Truncate(logLevel.ToString(), 16),
            Message = Truncate(message, 4000),
            Exception = exception != null ? Truncate(exception.ToString(), 8000) : null,
            ErrorCode = Truncate(errorCode, 64),
            TraceId = traceId?.ToString(),
            UserId = userId?.ToString(),
            UserName = userName?.ToString(),
            Path = path?.ToString(),
            Method = method?.ToString(),
            Ip = ip?.ToString(),
            UserAgent = userAgent?.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            //using var scope = _scopeFactory.CreateScope();
            //var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            AppDbContext context = new();
            context.AppLogEntries.Add(entry);
            context.SaveChanges();
        }
        catch
        {
            // Evita que falha ao gravar log quebre a aplicação
        }
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
