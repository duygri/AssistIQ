using AssistIQ.Application.Abstractions;
using AssistIQ.Infrastructure.Ai;
using AssistIQ.Infrastructure.Auth;
using AssistIQ.Infrastructure.Persistence;
using AssistIQ.Infrastructure.Persistence.Seed;
using AssistIQ.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AssistIQDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<UsageCostOptions>(builder.Configuration.GetSection("UsageCost"));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ISystemClock, SystemClock>();
builder.Services.AddScoped<IKnowledgeIndexer, FakeKnowledgeIndexer>();
builder.Services.AddScoped<IRetrievalService, FakeRetrievalService>();
builder.Services.AddScoped<IAiDraftService, FakeAiDraftService>();
builder.Services.AddScoped<IUsageRecorder, UsageRecorder>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<DemoDataSeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    service = "AssistIQ.Api",
    status = "ok"
}));

if (app.Configuration.GetValue<bool>("SeedDemoDataOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();

public partial class Program;
