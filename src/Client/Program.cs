using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.Blazor.Client.Services.Auth;
using MicroBlog.Blazor.Client.Services.Http;
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
            builder.Services.AddLogging();

            // a service to show toast notifications/messages in ui
            builder.Services.AddScoped<ToastService>();
            // custom Http Interceptor service, to handle http requests requirements.
            builder.Services.AddScoped<HttpInterceptorService>();
            // enable auth features
            builder.Services.AddAuthorizationCore();
            // service for working with client browser storage
            builder.Services.AddBlazoredLocalStorage();
            // http interceptor service
            builder.Services.AddHttpClientInterceptor();
            // loading progress service, used to track requests progress
            builder.Services.AddLoadingBar();

            #region Auth Service and providers
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
            #endregion

            #region MicroBlog Api Typed HttpClient Services & Configurations
            builder.Services.AddHttpClient<IAccountsClient, AccountsClient>("AccountsClient", (sp, cl) =>
            {
                cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                cl.EnableIntercept(sp);
            }).AddHttpMessageHandler<HttpInterceptorService>(); // add delegate handler

            builder.Services.AddHttpClient<IBlogClient, BlogClient.BlogClient>("BlogClient", (sp, cl) =>
             {
                 cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                 cl.EnableIntercept(sp);
             }).AddHttpMessageHandler<HttpInterceptorService>(); // add delegate handler

            builder.Services.AddHttpClient<IUploadClient, UploadClient>("UploadClient", (sp, cl) =>
             {
                 cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
                 cl.EnableIntercept(sp);
             }).AddHttpMessageHandler<HttpInterceptorService>(); // add delegate handler
            #endregion

            await builder.Build()
                .UseLoadingBar() // display a loading progress for http requests on ui
                .RunAsync();
        }
    }
}
