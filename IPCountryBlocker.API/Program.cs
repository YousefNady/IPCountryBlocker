using IPCountryBlocker.API.BackgroundServices;
using IPCountryBlocker.API.Interfaces;
using IPCountryBlocker.API.Middlewares;
using IPCountryBlocker.API.Repositories;
using IPCountryBlocker.API.Services;
namespace IPCountryBlocker.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DI for HttpContext
            builder.Services.AddHttpContextAccessor();

            // DI for Repositories (Singleton because they hold in-memory state for the app lifetime)
            builder.Services.AddSingleton<ICountryRepository, InMemoryCountryRepository>();
            builder.Services.AddSingleton<ILogRepository, InMemoryLogRepository>();

            // Register HttpClient and GeoLocationService
            builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>();

            // Register Background Services
            builder.Services.AddHostedService<TemporalBlockCleanupService>();

            var webApp = builder.Build();

            // Configure the HTTP request pipeline.
            if (webApp.Environment.IsDevelopment())
            {
                webApp.UseSwagger();
                webApp.UseSwaggerUI();
            }

            webApp.UseMiddleware<ExceptionHandlingMiddleware>();

            webApp.UseHttpsRedirection();

            webApp.UseAuthorization();

            webApp.MapControllers();

            webApp.Run();
        }
    }
}
