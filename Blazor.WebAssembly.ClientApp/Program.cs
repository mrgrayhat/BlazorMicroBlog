using System;
using System.Threading.Tasks;
using Blazor.WebAssembly.ClientApp.Services;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.WebAssembly.ClientApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            // register app taost notifications service.
            builder.Services.AddScoped<ToastService>();
            // if you want to run blazor client standalone (only wasm), set base address to server(api) uri, otherwise server will proxy it's self address (Hosted Mode).
            // default is https://127.0.0.1:5001 | http 5000
            builder.Services.AddHttpClient<IBlogClient, BlogClient>((cfg) =>
            {
                cfg.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });

            await builder.Build().RunAsync();
        }
    }
}
