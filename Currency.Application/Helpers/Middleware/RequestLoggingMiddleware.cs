using Currency.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Extensions
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IClaimService _claimService;
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger,IClaimService claimService)
        {
            _next = next;
            _logger = logger;
            _claimService = claimService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Capture request info
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var clientId = _claimService.GetClientId();
            var method = context.Request.Method;
            var path = context.Request.Path;

            // Continue pipeline
            await _next(context);
            stopwatch.Stop();

            // Capture response info
            var statusCode = context.Response.StatusCode;

            // Structured log
            _logger.LogInformation("Request {Method} {Path} from {ClientIp} ClientId={ClientId} -> {StatusCode} in {Elapsed:0.000} ms",
                method, path, clientIp, clientId, statusCode, stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
