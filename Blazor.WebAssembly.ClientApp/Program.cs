using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.WebAssembly.ClientApp.Services;
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
            // if you want to run blazor client standalone (only wasm), set uri to server(api), otherwise server will proxy it's self address (Hosted Mode)..
            // default is http://127.0.0.1:5001 | https
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                //BaseAddress = new Uri("http://localhost:5001")
            });

            await builder.Build().RunAsync();
        }
    }
}
