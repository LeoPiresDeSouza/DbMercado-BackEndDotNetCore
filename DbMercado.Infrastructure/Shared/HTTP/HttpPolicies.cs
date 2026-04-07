using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

namespace DbMercado.Infrastructure.Shared.HTTP;

public static class HttpPolicies
{
    /// <summary>
    /// Retentativas para OpenRouter: 3 repetições após a primeira chamada (até 4 envios), espera 1s, 2s e 4s.
    /// Registra cada falha antes de aguardar o backoff.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> OpenRouterTranslationRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(static r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, delay, retryAttempt, _) =>
                {
                    var motivo = outcome.Exception is not null
                        ? $"{outcome.Exception.GetType().Name}: {outcome.Exception.Message}"
                        : $"HTTP {(int)outcome.Result.StatusCode} {outcome.Result.ReasonPhrase}";
                    logger.LogWarning(
                        "OpenRouter HTTP: tentativa com falha {RetryIndex}/3 antes do backoff. Motivo: {Motivo}. Aguardando {DelaySegundos:F0}s.",
                        retryAttempt,
                        motivo,
                        delay.TotalSeconds);
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retry => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retry))
            );
    }

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30)
            );
    }
}
