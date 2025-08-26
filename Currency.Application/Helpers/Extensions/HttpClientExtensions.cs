using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net;

public static class PollyRetryExtensions
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.BadGateway)
            .Or<TaskCanceledException>() // includes request timeouts
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (outcome, timespan, retryAttempt, context) =>
                {
                    var reason = outcome.Exception?.Message
                                 ?? $"{(int)outcome.Result!.StatusCode} {outcome.Result.StatusCode}";
                    logger.LogWarning("Retry {RetryAttempt} after {Delay} due to {Reason}",
                        retryAttempt, timespan, reason);
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError().OrResult(r => r.StatusCode == HttpStatusCode.BadGateway)
            .CircuitBreakerAsync(
                5, // break after 5 failures
                TimeSpan.FromSeconds(30), // break duration
                onBreak: (outcome, timespan) =>
                {
                    var reason = outcome.Exception?.Message
                                 ?? $"{(int)outcome.Result!.StatusCode} {outcome.Result.StatusCode}";
                    logger.LogError("Circuit broken for {TimeSpan} due to {Reason}", timespan, reason);
                },
                onReset: () => logger.LogInformation("Circuit reset."),
                onHalfOpen: () => logger.LogInformation("Circuit half-open, next call is trial.")
            );
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(ILogger logger)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            10, // timeout in seconds
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (context, timespan, task, exception) =>
            {
                logger.LogError("Execution timed out after {TimeoutSeconds} seconds.", timespan.TotalSeconds);
                return Task.CompletedTask;
            });
    }
}
