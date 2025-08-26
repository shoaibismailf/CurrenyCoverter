using Currency.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Middleware
{
    public class UserRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserRateLimitMiddleware> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IClaimService _claimService;

        public UserRateLimitMiddleware(RequestDelegate next, ILogger<UserRateLimitMiddleware> logger, IConnectionMultiplexer redis, IClaimService claimService)
        {
            _next = next;
            _logger = logger;
            _redis = redis;
            _claimService = claimService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Ensure user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get user ID or client ID
            string? userId = _claimService.GetClientId();


            int limit = _claimService.GetRateLimit();
            int windowSeconds = 60;

            var db = _redis.GetDatabase();
            var key = $"ratelimit:user:{userId}";

            var count = await db.StringIncrementAsync(key);
            if (count == 1)
                await db.KeyExpireAsync(key, TimeSpan.FromSeconds(windowSeconds));

            if (count > limit)
            {
                _logger.LogWarning("User {UserId} exceeded rate limit of {Limit} requests in {WindowSeconds}s. Current count: {Count}",
                    userId, limit, windowSeconds, count);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = windowSeconds.ToString();

                var errorResponse = new
                {
                    Error = "RateLimitExceeded",
                    Message = $"You have exceeded the allowed {limit} requests per {windowSeconds} seconds. Please try again later.",
                    Limit = limit,
                    WindowSeconds = windowSeconds,
                    RetryAfterSeconds = windowSeconds
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }

            _logger.LogInformation("User {UserId} request allowed. Count {Count}/{Limit} in {WindowSeconds}s window.",
                userId, count, limit, windowSeconds);

            await _next(context);
        }
    }
}
