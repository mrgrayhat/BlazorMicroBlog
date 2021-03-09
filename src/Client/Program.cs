using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.Blazor.Client.Services.Auth;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

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
            builder.Services.AddHttpClientInterceptor();
            builder.Services.AddLoadingBar();

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

            builder.Services.AddHttpClient<IAccountsClient, AccountsClient>((sp, cl) =>
            {
                cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                cl.EnableIntercept(sp);
            });
            builder.Services.AddHttpClient<IBlogClient, BlogClient.BlogClient>((sp, cl) =>
            {
                cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                cl.EnableIntercept(sp);
            });
            builder.Services.AddHttpClient<IUploadClient, UploadClient>((sp, cl) =>
            {
                cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                cl.EnableIntercept(sp);
            });

            await builder.Build().UseLoadingBar().RunAsync();
        }
    }
}
