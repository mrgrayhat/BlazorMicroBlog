using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Server.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            })
                .AddJsonOptions((options) =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    //options.JsonSerializerOptions.WriteIndented = true;
                });
            //services.AddResponseCompression();
            services.AddResponseCaching();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
