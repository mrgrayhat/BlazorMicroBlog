using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.Blazor.Client.Services;
using MicroBlog.Blazor.Client.Services.Auth;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBlog.Blazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped<ToastService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

            builder.Services.AddHttpClient<IAccountsClient, AccountsClient>((cfg) =>
            {
                cfg.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });
            builder.Services.AddHttpClient<IBlogClient, BlogClient.BlogClient>((cfg) =>
            {
                cfg.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });
            builder.Services.AddHttpClient<IUploadClient, UploadClient>((cfg) =>
            {
                cfg.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });

            await builder.Build().RunAsync();
        }
    }
}
