using System;
using System.Threading.Tasks;
using MicroBlog.Server.API.Infrastructure.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MicroBlog.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var dbInitializer = scope.ServiceProvider.GetService<IDbInitializerService>();
                try
                {
                    logger.LogInformation("Seeding Blog Database");

                    await dbInitializer.Initialize(true); // true to ReCreate db or for test
                    await dbInitializer.SeedData();
                }
                catch (Exception ex)
                {
                    logger.LogError("Error creating/seeding API database - " + ex);
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
