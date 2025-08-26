using Microsoft.AspNetCore.Builder;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Currency.Application.Helpers.Middleware
{
    public static class SerilogContextMiddleware
    {
        public static IApplicationBuilder UseCurrencyLogContext(this IApplicationBuilder app)
        {
            return app.Use(async (ctx, next) =>
            {
                // ensure correlation id header exists (align with trace id if possible)
                var corr = ctx.Request.Headers["X-Correlation-ID"].ToString();
                if (string.IsNullOrWhiteSpace(corr))
                {
                    corr = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
                    ctx.Response.Headers["X-Correlation-ID"] = corr;
                }

                // (optional) enrich with user/provider via your IClaimService
                var claims = ctx.RequestServices.GetService<Currency.Application.Interfaces.IClaimService>();
                var userId = claims?.GetUserId() ?? claims?.GetClientId();
                var provider = claims?.GetCurrencyProvider();

                using (LogContext.PushProperty("CorrelationId", corr))
                using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
                using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString()))
                using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
                using (LogContext.PushProperty("CurrencyProvider", provider ?? "n/a"))
                {
                    await next();
                }
            });
        }
    }
}
