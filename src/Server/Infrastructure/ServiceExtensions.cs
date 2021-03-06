using MicroBlog.Server.API.Infrastructure.Contexts;
using MicroBlog.Server.API.Infrastructure.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace MicroBlog.Server.API.Infrastructure
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// add database services, swagger service configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environment"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructures(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
        {
            services.AddCustomCors();
            services.AddCustomSwagger();
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

        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            #region Swagger & OpenApi Configuration

            services.AddOpenApiDocument(configure =>
            {
                configure.Title = "Micro Blog API";
                configure.AddSecurity("JWT", Enumerable.Empty<string>(),
                    new NSwag.OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}."
                    });

                configure.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "BlazorMicroBlog.Api",
                    Version = "v1"
                });
            });
            #endregion
            return services;
        }
    }
}
