using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Use incoming correlation ID if present, else generate a new one
            if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers["X-Correlation-ID"] = correlationId;
            }

            context.Response.Headers["X-Correlation-ID"] = correlationId;

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
