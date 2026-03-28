using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using System.Net;

namespace DbMercado.Infrastructure.Shared.HTTP;

public static class HttpPolicies
{
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
