using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Toolbelt.Blazor;

namespace MicroBlog.Blazor.Client.Services.Http
{
    public class HttpInterceptorService : DelegatingHandler
    {
        private readonly HttpClientInterceptor _interceptor;
        private readonly NavigationManager _navManager;
        private readonly ILocalStorageService _localStorage;
        private readonly ToastService _toastService;

        public HttpInterceptorService(ILocalStorageService localStorage, HttpClientInterceptor interceptor, NavigationManager navManager, ToastService toastService)
        {
            _toastService = toastService;
            _localStorage = localStorage;
            _interceptor = interceptor;
            _navManager = navManager;
        }

        #region Response handler event register and dispose delegates
        public void RegisterEvent() => _interceptor.AfterSend += InterceptResponse;
        public void DisposeEvent() => _interceptor.AfterSend -= InterceptResponse;
        #endregion

        /// <summary>
        /// execute before any http request (for types/http clients that registered this interceptor)
        /// </summary>
        /// <param name="request">the sending request</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // for browser to send cookies and auth headers
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            // check auth token exist on http request or not.
            if (!request.Headers.Contains("bearer"))
            {
                // set auht token for request
                await SetToken(request, cancellationToken);
            }

            // refresh token check
            //var absPath = e.Request.RequestUri.AbsolutePath;
            //if (!absPath.Contains("token") && !absPath.Contains("accounts"))
            //{
            //    var token = await _refreshTokenService.TryRefreshToken();
            //if (!string.IsNullOrEmpty(token))
            //{
            //    e.Request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            //}

            RegisterEvent();
            var response = await base.SendAsync(request, cancellationToken);
            DisposeEvent();
            return response;
        }

        private void InterceptResponse(object sender, HttpClientInterceptorEventArgs e)
        {
            Console.WriteLine("InterceptResponse: " + e.Response.ReasonPhrase);
            if (!e.Response.IsSuccessStatusCode)
            {
                string message = string.Empty;
                switch (e.Response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        _navManager.NavigateTo("/404");
                        message = "The requested resorce was not found.";
                        _toastService.ShowToast(message, ToastLevel.WARNING);
                        break;
                    case HttpStatusCode.BadRequest:
                        message = "something is wrong!";
                        _toastService.ShowToast(message, ToastLevel.WARNING);
                        break;
                    case HttpStatusCode.Unauthorized:
                        _navManager.NavigateTo("/account/login");
                        message = "User is not authorized";
                        _toastService.ShowToast(message, ToastLevel.ERROR);
                        break;
                    default:
                        _navManager.NavigateTo("/500");
                        message = "Something went wrong, please contact Administrator";
                        _toastService.ShowToast(message, ToastLevel.ERROR);
                        break;
                }
                throw new HttpResponseException(message);
            }
        }

        /// <summary>
        /// get user token from browser local storage, if exist
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetToken()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }
        /// <summary>
        /// set auth token to request header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetToken(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var token = await GetToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            }
        }

    }
}
