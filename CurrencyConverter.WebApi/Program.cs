using Currency.Application.Helpers.Extensions;
using Currency.Application.Helpers.Observability;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCurrencyProviders(builder.Configuration);
builder.Services.AddIdentityServerConfig(builder.Configuration);
builder.Services.AddSwaggerWithAuth();
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseCurrencyLogging(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIdentityServer();

app.UseHttpsRedirection();

app.UseAuthorization();

app.AddMiddlewaresLogging();

app.MapControllers().RequireAuthorization("ApiScopes");

app.Run();
