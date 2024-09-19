using LightSensorApp.Db;
using LightSensorApp.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using LightSensorApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LightSensorContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TelemetryDatabase"), 
    b => b.MigrationsAssembly("LightSensorApp")));

builder.Services.AddScoped<ITelemetryService, TelemetryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/logError-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Error)
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();



app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
