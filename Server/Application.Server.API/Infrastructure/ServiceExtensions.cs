using Application.Server.API.Infrastructure.Contexts;
using Application.Server.API.Infrastructure.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace Application.Server.API.Infrastructure
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructures(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            services.AddCustomCors();

            services.AddDbContext<BlogDbContext>(options =>
            {
                bool.TryParse(configuration["Blog:UseSqLite"], out bool useSqlite);
                bool.TryParse(configuration["Blog:UseInMemory"], out bool useInMemory);
                string connectionString = configuration["Blog:ConnectionString"];

                if (useInMemory)
                {
                    options.UseInMemoryDatabase("MicroBlog"); // Takes database name
                }
                else if (useSqlite)
                {
                    options.UseSqlite(connectionString, b =>
                    {
                        b.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                    });
                    // ignore sqlite transaction warning
                    options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
                }
                else
                {
                    options.UseSqlServer(connectionString, b =>
                    {
                        b.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                    });

                }
                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });


            services.AddScoped<IBlogDbContext>(provider =>
            provider.GetService<BlogDbContext>());
            services.AddScoped<IDbInitializerService, DbInitializerService>();

            return services;
        }

        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCustomCors();
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            return services;
        }
    }
}
