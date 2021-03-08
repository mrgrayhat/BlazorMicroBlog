using System.Linq;
using System.Text;
using MicroBlog.Server.API.Infrastructure.Contexts;
using MicroBlog.Server.API.Infrastructure.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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
            services.AddCustomIdentity(configuration);

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

        private static IServiceCollection AddCustomCors(this IServiceCollection services)
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

        private static IServiceCollection AddCustomSwagger(this IServiceCollection services)
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

        /// <summary>
        /// add identity and configure user security options. Configure Token Authentication and Validation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection IdentityOptions = configuration.GetSection("IdentityOptions");
            IConfigurationSection jwtSettings = configuration.GetSection("JwtSettings");

            services.AddIdentity<IdentityUser, IdentityRole>((opt) =>
            {
                opt.User.RequireUniqueEmail = IdentityOptions.GetValue<bool>("RequireUniqueEmail");
                opt.Lockout.MaxFailedAccessAttempts = IdentityOptions.GetValue<int>("MaxFailedAccessAttempts");

                if (IdentityOptions.GetValue<bool>("HighComplexity") is false)
                {
                    opt.Password.RequiredUniqueChars = 0;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireUppercase = false;
                }
            }).AddEntityFrameworkStores<BlogDbContext>();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.GetValue<string>("validIssuer"),
                    ValidAudience = jwtSettings.GetValue<string>("validAudience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("securityKey")))
                };
            });
            return services;
        }
    }
}
