using Currency.Application.Helpers.Middleware;
using Currency.Application.Helpers.Observability;
using Currency.Application.Interfaces;
using Currency.Application.Interfaces.Redis;
using Currency.Application.Services;
using Currency.Application.Services.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using StackExchange.Redis;
using System.Net;

namespace Currency.Application.Helpers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrencyProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddTransient<IClaimService, ClaimService>();

            services.AddHttpClient<FrankFurterProviderService>()
                .AddPolicyHandler((sp, _) =>
                    PollyRetryExtensions.GetRetryPolicy(sp.GetRequiredService<ILogger<FrankFurterProviderService>>()))
                .AddPolicyHandler((sp, _) =>
                    PollyRetryExtensions.GetCircuitBreakerPolicy(sp.GetRequiredService<ILogger<FrankFurterProviderService>>()))
                .AddPolicyHandler((sp, _) =>
                    PollyRetryExtensions.GetTimeoutPolicy(sp.GetRequiredService<ILogger<FrankFurterProviderService>>()));

            services.AddTransient<OpenExchangeRatesProviderService>();
            services.AddScoped<IProviderFactoryService, ProviderFactoryService>();
            services.AddScoped<IRedisCacheService, RedisCacheService>();

            return services;
        }

        public static IHostBuilder UseCurrencyLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithCorrelationId()
                .Enrich.WithClientIp() 
                .Enrich.With(new ActivityTraceEnricher())
                .WriteTo.Console()
                .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "C:/Logs/log-.json", rollingInterval: RollingInterval.Day)
                .WriteTo.Seq(configuration["Seq:Url"]!)
                .CreateLogger();

            return hostBuilder.UseSerilog();
        }

        public static IApplicationBuilder AddMiddlewaresLogging(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<UserRateLimitMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            return app;
        }

        public static IServiceCollection AddIdentityServerConfig(this IServiceCollection services, IConfiguration _configuration)
        {
            services.AddIdentityServer()
                .AddInMemoryClients(Config.Clients)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddTestUsers(Config.Users)
                .AddDeveloperSigningCredential();


            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = _configuration["IdentityServerConfig:Authority"]!;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = "currency_api",
                        NameClaimType = "name",
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScopes", policy =>
                    policy.RequireAssertion(ctx =>
                    {
                        // Collect both 'scope' and 'scp' claims (IS/duende vs Azure style)
                        var rawScopes = ctx.User.FindAll("scope").Select(c => c.Value)
                            .Concat(ctx.User.FindAll("scp").Select(c => c.Value));

                        foreach (var raw in rawScopes)
                        {
                            // Some tokens have multiple scopes in one claim separated by spaces
                            var scopes = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (scopes.Any(s => s.StartsWith("currency_api.", StringComparison.OrdinalIgnoreCase)))
                                return true;
                        }
                        return false;
                    }));
            });
            return services;
        }

        public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration _configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _configuration["RedisServerConfig:Authority"]!;
            });

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(_configuration["RedisServerConfig:Authority"]!));
            return services;
        }
    }
}
