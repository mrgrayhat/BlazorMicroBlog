using Application.Server.API.Infrastructure.Contexts;
using Application.Server.API.Infrastructure.Seeds;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Application.Server.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            HostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(opt =>
            {
                opt.ReturnHttpNotAcceptable = true;
                opt.CacheProfiles.Add("30SecCache", new CacheProfile
                {
                    Duration = 30
                });
            }).AddJsonOptions((options) =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    //options.JsonSerializerOptions.WriteIndented = true;
                });
            //services.AddResponseCompression();
            services.AddResponseCaching();
            services.AddCors();
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

            services.AddDbContext<BlogDbContext>(options =>
            {
                bool.TryParse(Configuration["Blog:UseSqLite"], out bool useSqlite);
                bool.TryParse(Configuration["Blog:UseInMemory"], out bool useInMemory);
                string connectionString = Configuration["Blog:ConnectionString"];

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
                    options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
                }
                else
                {
                    options.UseSqlServer(connectionString, b =>
                    {
                        b.MigrationsAssembly(typeof(BlogDbContext).Assembly.FullName);
                    });

                }
                if (HostingEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });


            services.AddScoped<IBlogDbContext>(provider =>
            provider.GetService<BlogDbContext>());

            services.AddScoped<IDbInitializerService, DbInitializerService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseResponseCompression();
            app.UseCors(configurePolicy: (opt) =>
            {
                opt.AllowAnyOrigin();
                opt.AllowAnyMethod();
                opt.AllowAnyHeader();
            });
            app.UseSerilogRequestLogging();
            app.UseResponseCaching();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
